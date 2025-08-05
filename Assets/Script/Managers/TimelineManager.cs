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
    // Only the cells that changed since the previous snapshot
    public Dictionary<Vector2Int, DTO.MapCellDTO> cellDeltas;
    // Only the resources that changed since the previous snapshot
    public Dictionary<string, GameResources> resourceDeltas;
    // Characters are stored in full for now
    public List<DTO.CharacterDTO> characters;
}

// Represents a fully reconstructed world state at a given time
public class WorldState
{
    public Dictionary<Vector2Int, DTO.MapCellDTO> cells = new();
    public Dictionary<string, GameResources> resources = new();
    public List<DTO.CharacterDTO> characters = new();
}

public class TimelineManager : MonoBehaviour
{
    public static TimelineManager Instance { get; private set; }

    private List<WorldEvent> globalEvents = new();
    private List<WorldEvent> currentTimelineEvents = null;
    private Dictionary<string, List<WorldEvent>> entityLogs = new();
    private Dictionary<string, int> objectOrigins = new();
    private List<Snapshot> snapshots = new();
    // Cached full state of last snapshot for delta calculation
    private Dictionary<Vector2Int, DTO.MapCellDTO> lastSnapshotCells = new();
    private Dictionary<string, GameResources> lastSnapshotResources = new();
    private Dictionary<int, int> rngSeeds = new();
    private int nextId = 1;
    private bool isTimeTraveling = false;
    private float originalTime = 0f;

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

    public void BeginTimeTravelTo()
    {
        if (isTimeTraveling)
            return;

        originalTime = GameClock.Time;
        currentTimelineEvents = globalEvents
            .Where(e => e.timestamp <= originalTime)
            .ToList();
        isTimeTraveling = true;
    }

    public void FinishTimeTravel()
    {
        if (!isTimeTraveling)
            return;

        globalEvents = currentTimelineEvents ?? new List<WorldEvent>();
        RebuildEntityLogs();

        snapshots = snapshots.Where(s => s.timestamp <= originalTime).ToList();
        RebuildLastState();
        SaveSnapshot(true);

        isTimeTraveling = false;
        currentTimelineEvents = null;

        GameTimeManager.UpdateDateFromSeconds(originalTime);
    }

    public void RequestDefensiveJoin()
    {
        Debug.Log("Defensive join requested.");
    }

    public WorldEvent RecordEvent(string actorId, string action, Dictionary<string, string> parameters, List<int> deps = null, int? rngSeed = null)
    {
        var ev = new WorldEvent(nextId++, GameClock.Time, actorId, action);
        if (parameters != null)
        {
            foreach (var kv in parameters)
                ev.parameters[kv.Key] = kv.Value;
        }
        if (deps != null)
            ev.dependencies.AddRange(deps);

        if (isTimeTraveling)
            currentTimelineEvents.Add(ev);
        else
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

    bool CellsEqual(DTO.MapCellDTO a, DTO.MapCellDTO b)
    {
        if (a == null && b == null) return true;
        if (a == null || b == null) return false;
        if (a.terrain != b.terrain) return false;
        if (a.building != b.building) return false;
        if (a.owner != b.owner) return false;
        if (a.level != b.level) return false;
        if (a.start_player != b.start_player) return false;
        if (a.resources == null && b.resources == null) return true;
        if (a.resources == null || b.resources == null) return false;
        return a.resources.gold == b.resources.gold &&
               a.resources.wood == b.resources.wood &&
               a.resources.crono == b.resources.crono;
    }

    bool ResourcesEqual(GameResources a, GameResources b)
    {
        if (a == null && b == null) return true;
        if (a == null || b == null) return false;
        return a.gold == b.gold && a.wood == b.wood && a.food == b.food &&
               a.crono == b.crono && a.science == b.science &&
               a.freeHousing == b.freeHousing &&
               a.academicUnits == b.academicUnits &&
               a.barracksUnits == b.barracksUnits;
    }

    public WorldState GetWorldStateAt(float time)
    {
        var world = new WorldState();
        List<DTO.CharacterDTO> chars = null;

        foreach (var snap in snapshots.OrderBy(s => s.timestamp))
        {
            if (snap.timestamp > time)
                break;

            if (snap.cellDeltas != null)
            {
                foreach (var kv in snap.cellDeltas)
                    world.cells[kv.Key] = kv.Value.Clone();
            }

            if (snap.resourceDeltas != null)
            {
                foreach (var kv in snap.resourceDeltas)
                    world.resources[kv.Key] = CloneResources(kv.Value);
            }

            if (snap.characters != null)
                chars = new List<DTO.CharacterDTO>(snap.characters);
        }

        MapState.cellMap = world.cells.ToDictionary(kv => kv.Key, kv => kv.Value.Clone());
        if (world.resources.TryGetValue("player", out var res))
            GameState.playerResources = CloneResources(res);
        else
            GameState.playerResources = new GameResources();
        MapLoader.instance?.ReloadFromState();

        if (MapLoader.instance != null)
        {
            foreach (var ch in GameObject.FindObjectsOfType<Character>())
                GameObject.Destroy(ch.gameObject);

            if (chars != null)
            {
                foreach (var c in chars)
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

        world.characters = chars ?? new List<DTO.CharacterDTO>();
        return world;
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

        var cellDeltas = new Dictionary<Vector2Int, DTO.MapCellDTO>();
        foreach (var kv in MapState.cellMap)
        {
            if (!lastSnapshotCells.TryGetValue(kv.Key, out var prev) || !CellsEqual(kv.Value, prev))
                cellDeltas[kv.Key] = kv.Value.Clone();
        }

        var currentResources = new Dictionary<string, GameResources>
        {
            ["player"] = GameState.playerResources
        };
        var resourceDeltas = new Dictionary<string, GameResources>();
        foreach (var kv in currentResources)
        {
            if (!lastSnapshotResources.TryGetValue(kv.Key, out var prevRes) || !ResourcesEqual(kv.Value, prevRes))
                resourceDeltas[kv.Key] = CloneResources(kv.Value);
        }

        var snap = new Snapshot
        {
            timestamp = GameClock.Time,
            cellDeltas = cellDeltas,
            resourceDeltas = resourceDeltas,
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

        lastSnapshotCells = MapState.cellMap.ToDictionary(
            entry => entry.Key,
            entry => entry.Value.Clone()
        );
        lastSnapshotResources = currentResources.ToDictionary(
            entry => entry.Key,
            entry => CloneResources(entry.Value)
        );
    }

    public void RemoveLastSnapshot()
    {
        if (snapshots.Count > 0)
        {
            snapshots.RemoveAt(snapshots.Count - 1);
            RebuildLastState();
        }
    }

    void RebuildLastState()
    {
        lastSnapshotCells = new Dictionary<Vector2Int, DTO.MapCellDTO>();
        lastSnapshotResources = new Dictionary<string, GameResources>();
        foreach (var snap in snapshots.OrderBy(s => s.timestamp))
        {
            if (snap.cellDeltas != null)
            {
                foreach (var kv in snap.cellDeltas)
                    lastSnapshotCells[kv.Key] = kv.Value.Clone();
            }
            if (snap.resourceDeltas != null)
            {
                foreach (var kv in snap.resourceDeltas)
                    lastSnapshotResources[kv.Key] = CloneResources(kv.Value);
            }
        }
    }

    void RebuildEntityLogs()
    {
        entityLogs = new Dictionary<string, List<WorldEvent>>();
        foreach (var ev in globalEvents)
        {
            if (!entityLogs.ContainsKey(ev.actorId))
                entityLogs[ev.actorId] = new List<WorldEvent>();
            entityLogs[ev.actorId].Add(ev);
        }
    }

    public List<Snapshot> GetSnapshots()
    {
        return new List<Snapshot>(snapshots);
    }

    public void SetSnapshots(List<Snapshot> snaps)
    {
        snapshots = snaps ?? new List<Snapshot>();
        RebuildLastState();
    }
}
