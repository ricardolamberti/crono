using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class WorldEvent
{
    public int id;
    public float timestamp;
    public string actorId;
    public string action;
    public Dictionary<string, string> parameters = new();
    public List<int> dependencies = new();

    public WorldEvent(int id, float timestamp, string actorId, string action)
    {
        this.id = id;
        this.timestamp = timestamp;
        this.actorId = actorId;
        this.action = action;
    }
}

public class Snapshot
{
    public float timestamp;
    public Dictionary<Vector2Int, DTO.MapCellDTO> cells;
    public Dictionary<string, GameResources> resources;
    public List<DTO.CharacterDTO> characters;
}

public class TimelineManager : MonoBehaviour
{
    public static TimelineManager Instance { get; private set; }

    private List<WorldEvent> globalEvents = new();
    private Dictionary<string, List<WorldEvent>> entityLogs = new();
    private Dictionary<string, int> objectOrigins = new();
    private List<Snapshot> snapshots = new();
    private Dictionary<int, int> rngSeeds = new();
    private int nextId = 1;

    void Awake()
    {
        Instance = this;
    }

    void OnEnable()
    {
        GameTimeManager.OnDateChanged += OnDateChanged;
    }

    void OnDisable()
    {
        GameTimeManager.OnDateChanged -= OnDateChanged;
    }

    void OnDateChanged(int month, int year)
    {
        SaveSnapshot(false);
    }

    public WorldEvent RecordEvent(string actorId, string action, Dictionary<string, string> parameters, List<int> deps = null, int? rngSeed = null)
    {
        var ev = new WorldEvent(nextId++, Time.time, actorId, action);
        if (parameters != null)
        {
            foreach (var kv in parameters)
                ev.parameters[kv.Key] = kv.Value;
        }
        if (deps != null)
            ev.dependencies.AddRange(deps);

        globalEvents.Add(ev);
        if (!entityLogs.ContainsKey(actorId))
            entityLogs[actorId] = new List<WorldEvent>();
        entityLogs[actorId].Add(ev);
        if(rngSeed.HasValue) rngSeeds[ev.id]=rngSeed.Value;
        return ev;
    }

    public List<WorldEvent> GetEntityLog(string entityId)
    {
        if (entityLogs.TryGetValue(entityId, out var list))
            return new List<WorldEvent>(list);
        return new List<WorldEvent>();
    }

    public List<WorldEvent> TraceDependencies(string entityId)
    {
        List<WorldEvent> result = new();
        if (!entityLogs.TryGetValue(entityId, out var log))
            return result;
        Queue<int> q = new();
        HashSet<int> visited = new();
        foreach (var e in log)
        {
            q.Enqueue(e.id);
            visited.Add(e.id);
        }
        while (q.Count > 0)
        {
            int id = q.Dequeue();
            var ev = globalEvents.Find(e => e.id == id);
            if (ev != null)
            {
                result.Add(ev);
                foreach (int d in ev.dependencies)
                {
                    if (!visited.Contains(d))
                    {
                        visited.Add(d);
                        q.Enqueue(d);
                    }
                }
            }
        }
        return result;
    }

    public void RegisterObject(string objectId, WorldEvent source)
    {
        objectOrigins[objectId] = source.id;
    }

    public int GetOriginEvent(string objectId)
    {
        if (objectOrigins.TryGetValue(objectId, out var id))
            return id;
        return -1;
    }

    public int GetRngSeed(int eventId)
    {
        if(rngSeeds.TryGetValue(eventId, out var s))
            return s;
        return -1;
    }

    GameResources CloneResources(GameResources src)
    {
        return new GameResources
        {
            gold = src.gold,
            wood = src.wood,
            food = src.food,
            crono = src.crono,
            science = src.science,
            freeHousing = src.freeHousing,
            academicUnits = src.academicUnits,
            barracksUnits = src.barracksUnits
        };
    }

    public Snapshot GetWorldStateAt(float time)
    {
        Snapshot snap = null;
        for (int i = snapshots.Count - 1; i >= 0; i--)
        {
            if (snapshots[i].timestamp <= time)
            {
                snap = snapshots[i];
                break;
            }
        }
        if (snap != null)
        {
            MapState.cellMap = new Dictionary<Vector2Int, DTO.MapCellDTO>(snap.cells);
            if (snap.resources.TryGetValue("player", out var res))
                GameState.playerResources = CloneResources(res);
            MapLoader.instance?.ReloadFromState();

            if (MapLoader.instance != null)
            {
                foreach (var ch in GameObject.FindObjectsOfType<Character>())
                    GameObject.Destroy(ch.gameObject);

                if (snap.characters != null)
                {
                    foreach (var c in snap.characters)
                    {
                        var pos = new Vector2Int(c.x, c.y);
                        Character.Type type;
                        if (System.Enum.TryParse(c.type, out type))
                        {
                            MapLoader.instance.SpawnCharacter(pos, type, c.owner);
                        }
                        else
                        {
                            if (c.type == "worker" || c.type == "scientist" || c.type == "warrior")
                            {
                                if (c.type == "worker") type = Character.Type.Worker;
                                else if (c.type == "scientist") type = Character.Type.Scientist;
                                else type = Character.Type.Warrior;
                                MapLoader.instance.SpawnCharacter(pos, type, c.owner);
                            }
                        }
                    }
                }
            }
        }
        return snap;
    }

    public void SaveSnapshot(bool force)
    {

        int currentMonth = GameTimeManager.CurrentMonth;
        int currentYear = GameTimeManager.CurrentYear;

        // Buscar índice del snapshot existente para este año/mes
        int existingIndex = -1;
        
        if (!force)
            existingIndex= snapshots.FindIndex(s =>
            {
                GameTimeManager.SecondsToDate(s.timestamp, out currentMonth, out currentYear);
                return GameTimeManager.CurrentMonth == currentMonth && GameTimeManager.CurrentYear == currentYear;
            });

        var charList = new List<DTO.CharacterDTO>();
        foreach (var character in GameObject.FindObjectsOfType<Character>())
        {
            var p = character.GetGridPosition();
            string type = character.characterType.ToString();
            if (character.role != null)
                type = character.role.Code;
            charList.Add(new DTO.CharacterDTO
            {
                x = p.x,
                y = p.y,
                type = type,
                owner = character.owner
            });
        }

        var snap = new Snapshot
        {
            timestamp = Time.time,
            cells = MapState.cellMap.ToDictionary(
                entry => entry.Key,
                entry => entry.Value.Clone()
            ),
            resources = new Dictionary<string, GameResources>
            {
                ["player"] = GameState.playerResources.Clone()
            },
            characters = charList
        };

        if (existingIndex >= 0)
        {
            snapshots[existingIndex] = snap; // Sobrescribir
        }
        else
        {
            snapshots.Add(snap); // Agregar nuevo
        }
    }

    public void RemoveLastSnapshot()
    {
        if (snapshots.Count > 0)
            snapshots.RemoveAt(snapshots.Count - 1);
    }

    public List<Snapshot> GetSnapshots()
    {
        return new List<Snapshot>(snapshots);
    }

    public void SetSnapshots(List<Snapshot> snaps)
    {
        snapshots = snaps ?? new List<Snapshot>();
    }
}
