using System;
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

public class TimelineBranch
{
    public string branchId;
    public string? parentBranchId;
    public float branchPointTime;
    public List<WorldEvent> events = new();
    public List<Snapshot> snapshots = new();
    public bool isActive;
    public string createdByPlayer;
}

public class TimelineManager : MonoBehaviour
{
    public static TimelineManager Instance { get; private set; }

    private List<WorldEvent> currentTimelineEvents = null;
    private List<Snapshot> currentTimelineSnapshots = null;
    private Dictionary<string, List<WorldEvent>> entityLogs = new();
    private Dictionary<string, int> objectOrigins = new();
    private Dictionary<string, TimelineBranch> allBranches = new();
    private TimelineBranch currentBranch;
    // Cached full state of last snapshot for delta calculation
    private Dictionary<Vector2Int, DTO.MapCellDTO> lastSnapshotCells = new();
    private Dictionary<string, GameResources> lastSnapshotResources = new();
    private Dictionary<int, int> rngSeeds = new();
    private int nextId = 1;
    private bool isTimeTraveling = false;
    private float originalTime = 0f;
    private float timeTravelStartTime = 0f;

    void Awake()
    {
        Instance = this;
        var root = new TimelineBranch
        {
            branchId = Guid.NewGuid().ToString(),
            parentBranchId = null,
            branchPointTime = 0f,
            isActive = true,
            createdByPlayer = string.Empty
        };
        allBranches[root.branchId] = root;
        currentBranch = root;
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

    public void BeginTimeTravelTo(float targetTime)
    {
        if (isTimeTraveling)
            return;

        originalTime = GameClock.Time;
        timeTravelStartTime = targetTime;

        currentTimelineEvents = currentBranch.events
            .Where(e => e.timestamp <= targetTime)
            .ToList();

        currentTimelineSnapshots = new List<Snapshot>();

        GameClock.Set(targetTime);
        GameTimeManager.UpdateDateFromSeconds(targetTime);

        lastSnapshotCells = MapState.cellMap.ToDictionary(
            entry => entry.Key,
            entry => entry.Value.Clone()
        );
        lastSnapshotResources = new Dictionary<string, GameResources>
        {
            ["player"] = CloneResources(GameState.playerResources)
        };

        isTimeTraveling = true;
    }

    public void FinishTimeTravel()
    {
        if (!isTimeTraveling)
            return;

        var oldBranch = currentBranch;
        oldBranch.isActive = false;

        var newBranch = new TimelineBranch
        {
            branchId = Guid.NewGuid().ToString(),
            parentBranchId = oldBranch.branchId,
            branchPointTime = timeTravelStartTime,
            isActive = true,
            createdByPlayer = "player",
            events = currentTimelineEvents ?? new List<WorldEvent>(),
            snapshots = CloneAndAdaptSnapshotsFromOldBranch(oldBranch.snapshots, timeTravelStartTime)
        };

        if (currentTimelineSnapshots != null)
            newBranch.snapshots.AddRange(currentTimelineSnapshots);

        allBranches[newBranch.branchId] = newBranch;
        currentBranch = newBranch;

        RebuildEntityLogs();
        RebuildLastState();

        GameClock.Set(originalTime);
        GameTimeManager.UpdateDateFromSeconds(originalTime);

        isTimeTraveling = false;
        currentTimelineEvents = null;
        currentTimelineSnapshots = null;
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
            currentBranch.events.Add(ev);
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
            var ev = currentBranch.events.Find(e => e.id == id);
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

    List<Snapshot> CloneAndAdaptSnapshotsFromOldBranch(List<Snapshot> oldSnaps, float cutoff)
    {
        var list = new List<Snapshot>();
        foreach (var snap in oldSnaps.Where(s => s.timestamp < cutoff))
        {
            var clone = new Snapshot
            {
                timestamp = snap.timestamp,
                cellDeltas = snap.cellDeltas?.ToDictionary(kv => kv.Key, kv => kv.Value.Clone()),
                resourceDeltas = snap.resourceDeltas?.ToDictionary(kv => kv.Key, kv => CloneResources(kv.Value)),
                characters = snap.characters?.Select(c => new DTO.CharacterDTO { x = c.x, y = c.y, type = c.type, owner = c.owner }).ToList()
            };
            AdaptToNewPast(clone);
            list.Add(clone);
        }
        return list;
    }

    void AdaptToNewPast(Snapshot snap)
    {
        // Placeholder for adapting snapshot data based on new past events
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

        if (isTimeTraveling && time < timeTravelStartTime)
            timeTravelStartTime = time;

        var orderedSnapshots = currentBranch.snapshots.OrderBy(s => s.timestamp).ToList();
        bool anyApplied = false;

        for (int i = 0; i < orderedSnapshots.Count; i++)
        {
            var snap = orderedSnapshots[i];

            if (snap.timestamp > time)
            {
                // Si aún no aplicamos ningún snapshot, usamos el primero
                if (!anyApplied && orderedSnapshots.Count > 0)
                    snap = orderedSnapshots[0];
                else
                    break;
            }

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

            anyApplied = true;
        }

        if (!anyApplied && orderedSnapshots.Count > 0)
        {
            var first = orderedSnapshots[0];
            if (first.cellDeltas != null)
            {
                foreach (var kv in first.cellDeltas)
                    world.cells[kv.Key] = kv.Value.Clone();
            }

            if (first.resourceDeltas != null)
            {
                foreach (var kv in first.resourceDeltas)
                    world.resources[kv.Key] = CloneResources(kv.Value);
            }

            if (first.characters != null)
                chars = new List<DTO.CharacterDTO>(first.characters);
        }

        MapState.cellMap = world.cells.ToDictionary(kv => kv.Key, kv => kv.Value.Clone());
        GameState.playerResources = world.resources.TryGetValue("player", out var res)
            ? CloneResources(res)
            : new GameResources();

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
                    if (System.Enum.TryParse(c.type, out Character.Type type))
                    {
                        MapLoader.instance.SpawnCharacter(pos, type, c.owner);
                    }
                    else
                    {
                        if (c.type == "worker") type = Character.Type.Worker;
                        else if (c.type == "scientist") type = Character.Type.Scientist;
                        else if (c.type == "warrior") type = Character.Type.Warrior;
                        else continue;
                        MapLoader.instance.SpawnCharacter(pos, type, c.owner);
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
        {
            var targetList = isTimeTraveling ? currentTimelineSnapshots : currentBranch.snapshots;
            existingIndex = targetList.FindIndex(s =>
            {
                GameTimeManager.SecondsToDate(s.timestamp, out currentMonth, out currentYear);
                return GameTimeManager.CurrentMonth == currentMonth && GameTimeManager.CurrentYear == currentYear;
            });
        }

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

        if (isTimeTraveling)
        {
            if (existingIndex >= 0)
                currentTimelineSnapshots[existingIndex] = snap;
            else
                currentTimelineSnapshots.Add(snap);
        }
        else
        {
            if (existingIndex >= 0)
                currentBranch.snapshots[existingIndex] = snap; // Sobrescribir
            else
                currentBranch.snapshots.Add(snap); // Agregar nuevo
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
        if (currentBranch.snapshots.Count > 0)
        {
            currentBranch.snapshots.RemoveAt(currentBranch.snapshots.Count - 1);
            RebuildLastState();
        }
    }

    void RebuildLastState()
    {
        lastSnapshotCells = new Dictionary<Vector2Int, DTO.MapCellDTO>();
        lastSnapshotResources = new Dictionary<string, GameResources>();
        foreach (var snap in currentBranch.snapshots.OrderBy(s => s.timestamp))
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
        foreach (var ev in currentBranch.events)
        {
            if (!entityLogs.ContainsKey(ev.actorId))
                entityLogs[ev.actorId] = new List<WorldEvent>();
            entityLogs[ev.actorId].Add(ev);
        }
    }

    public List<Snapshot> GetSnapshots()
    {
        return new List<Snapshot>(currentBranch.snapshots);
    }

    public void SetSnapshots(List<Snapshot> snaps)
    {
        currentBranch.snapshots = snaps ?? new List<Snapshot>();
        RebuildLastState();
    }

    public TimelineBranch GetBranchById(string id)
    {
        allBranches.TryGetValue(id, out var branch);
        return branch;
    }

    public List<TimelineBranch> GetAllBranches()
    {
        return allBranches.Values.ToList();
    }
}
