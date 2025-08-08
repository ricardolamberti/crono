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
    // Indicates whether the event was applied when rebuilding a branch
    public bool wasApplied = false;
    // If not applied, stores the reason
    public string failureReason = null;

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
    public Snapshot baseSnapshot;
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
    // Stores events that were discarded when rebuilding a branch
    private List<WorldEvent> discardedEvents = new();
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
            baseSnapshot = new Snapshot
            {
                timestamp = 0f,
                cellDeltas = new Dictionary<Vector2Int, DTO.MapCellDTO>(),
                resourceDeltas = new Dictionary<string, GameResources>(),
                characters = new List<DTO.CharacterDTO>()
            },
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
        var baseSnap = CaptureCurrentStateSnapshot();

        var futureEvents = oldBranch.events
            .Where(e => e.timestamp > timeTravelStartTime && e.timestamp <= originalTime)
            .ToList();

        var allEvents = new List<WorldEvent>();
        if (currentTimelineEvents != null)
            allEvents.AddRange(currentTimelineEvents);
        allEvents.AddRange(futureEvents);
        allEvents = allEvents.OrderBy(e => e.timestamp).ToList();

        var newBranch = new TimelineBranch
        {
            branchId = Guid.NewGuid().ToString(),
            parentBranchId = oldBranch.branchId,
            branchPointTime = timeTravelStartTime,
            isActive = true,
            createdByPlayer = "player",
            baseSnapshot = baseSnap,
            events = allEvents,
            snapshots = new List<Snapshot>()
        };

        allBranches[newBranch.branchId] = newBranch;
        currentBranch = newBranch;

        isTimeTraveling = false;

        RebuildBranchMonthByMonth(baseSnap, allEvents, originalTime);
        RebuildEntityLogs();
        RebuildLastState();

        GameClock.Set(originalTime);
        GameTimeManager.UpdateDateFromSeconds(originalTime);

        currentTimelineEvents = null;
        currentTimelineSnapshots = null;
    }
    bool IsEventStillValid(WorldEvent ev)
    {
        if (ev.action == "place_building")
        {
            var x = int.Parse(ev.parameters["x"]);
            var y = int.Parse(ev.parameters["y"]);
            var pos = new Vector2Int(x, y);
            return MapLoader.instance.IsPositionFree(pos);
        }

        // Agregá validaciones específicas para otros tipos
        return true;
    }
    void ApplyEvent(WorldEvent ev)
    {
        if (ev.action == "build")
        {
            var code = ev.parameters["code"];
            var x = int.Parse(ev.parameters["x"]);
            var y = int.Parse(ev.parameters["y"]);
            var pos = new Vector2Int(x, y);
            var owner = ev.actorId;

            MapLoader.instance.PlaceBuilding(pos, code, owner);
        }

        // Otros casos: spawn_character, upgrade_building, etc.
    }

    bool TryApplyEvent(WorldEvent ev)
    {
        switch (ev.action)
        {
            case "build":
            case "place_building":
                return MapLoader.instance != null && MapLoader.instance.TryPlaceBuildingFromEvent(ev);
            case "spawn_character":
                return MapLoader.instance != null && MapLoader.instance.TrySpawnCharacterFromEvent(ev);
            case "collect_resource":
                return TryCollectResource(ev);
            default:
                return false;
        }
    }

    bool TryCollectResource(WorldEvent ev)
    {
        if (ev == null || ev.parameters == null)
            return false;
        if (!ev.parameters.TryGetValue("resource", out var res) || !ev.parameters.TryGetValue("amount", out var amt))
            return false;
        if (!int.TryParse(amt, out int amount) || amount <= 0)
            return false;

        switch (res)
        {
            case "gold":
                GameState.playerResources.gold += amount;
                return true;
            case "wood":
                GameState.playerResources.wood += amount;
                return true;
            case "food":
                GameState.playerResources.food += amount;
                return true;
            case "crono":
                GameState.playerResources.crono += amount;
                return true;
            case "science":
                GameState.playerResources.science += amount;
                return true;
            default:
                return false;
        }
    }

    bool IsEventValid(WorldEvent ev, HashSet<int> appliedEvents)
    {
        foreach (var dep in ev.dependencies)
        {
            if (!appliedEvents.Contains(dep))
                return false;
        }
        return true;
    }

    bool LoadSnapshot(Snapshot snap)
    {
        if (snap == null)
            return false;

        MapState.cellMap = snap.cellDeltas?.ToDictionary(kv => kv.Key, kv => kv.Value.Clone())
            ?? new Dictionary<Vector2Int, DTO.MapCellDTO>();

        if (snap.resourceDeltas != null && snap.resourceDeltas.TryGetValue("player", out var res))
            GameState.playerResources = CloneResources(res);
        else
            GameState.playerResources = new GameResources();

        foreach (var ch in GameObject.FindObjectsOfType<Character>())
            GameObject.Destroy(ch.gameObject);

        MapLoader.instance?.ReloadFromState();

        if (snap.characters != null)
        {
            foreach (var c in snap.characters)
            {
                var pos = new Vector2Int(c.x, c.y);
                if (Enum.TryParse<Character.Type>(c.type, true, out var t))
                    MapLoader.instance?.SpawnCharacter(pos, t, c.owner, false);
                else
                {
                    Character.Type t2;
                    if (c.type == "worker") t2 = Character.Type.Worker;
                    else if (c.type == "scientist") t2 = Character.Type.Scientist;
                    else if (c.type == "warrior") t2 = Character.Type.Warrior;
                    else continue;
                    MapLoader.instance?.SpawnCharacter(pos, t2, c.owner, false);
                }
            }
        }

        return true;
    }

    public bool RebuildBranchFromEvents(Snapshot baseSnapshot, List<WorldEvent> events)
    {
        if (!LoadSnapshot(baseSnapshot))
            return false;

        discardedEvents.Clear();
        var ordered = events.OrderBy(e => e.timestamp).ToList();
        HashSet<int> applied = new HashSet<int>();
        List<WorldEvent> survivors = new();

        foreach (var ev in ordered)
        {
            ev.wasApplied = false;
            ev.failureReason = null;

            if (!IsEventValid(ev, applied))
            {
                ev.failureReason = "missing_dependency";
                discardedEvents.Add(ev);
                Debug.Log($"[Timeline] Evento {ev.id} descartado por dependencias.");
                continue;
            }

            if (TryApplyEvent(ev))
            {
                ev.wasApplied = true;
                applied.Add(ev.id);
                survivors.Add(ev);
            }
            else
            {
                ev.failureReason = "validation_failed";
                discardedEvents.Add(ev);
                Debug.Log($"[Timeline] Evento {ev.id} descartado por estado inválido.");
            }
        }

        events.Clear();
        events.AddRange(survivors);
        RebuildEntityLogs();

        return true;
    }

    void RebuildBranchMonthByMonth(Snapshot baseSnapshot, List<WorldEvent> events, float endTime)
    {
        if (!LoadSnapshot(baseSnapshot))
            return;

        currentBranch.snapshots.Clear();

        lastSnapshotCells = MapState.cellMap.ToDictionary(
            entry => entry.Key,
            entry => entry.Value.Clone()
        );
        lastSnapshotResources = new Dictionary<string, GameResources>
        {
            ["player"] = CloneResources(GameState.playerResources)
        };

        discardedEvents.Clear();
        var ordered = events.OrderBy(e => e.timestamp).ToList();
        HashSet<int> applied = new HashSet<int>();
        List<WorldEvent> survivors = new();
        int idx = 0;

        float currentTime = baseSnapshot.timestamp;
        GameClock.Set(currentTime);
        GameTimeManager.UpdateDateFromSeconds(currentTime);
        GameTimeManager.SecondsToDate(currentTime, out int month, out int year);

        while (currentTime < endTime)
        {
            int nextMonth = month == 12 ? 1 : month + 1;
            int nextYear = month == 12 ? year + 1 : year;
            float nextTime = GameTimeManager.DateToSeconds(nextMonth, nextYear);
            float intervalEnd = Mathf.Min(nextTime, endTime);

            while (idx < ordered.Count && ordered[idx].timestamp <= intervalEnd)
            {
                var ev = ordered[idx++];
                ev.wasApplied = false;
                ev.failureReason = null;

                if (!IsEventValid(ev, applied))
                {
                    ev.failureReason = "missing_dependency";
                    discardedEvents.Add(ev);
                    continue;
                }

                if (TryApplyEvent(ev))
                {
                    ev.wasApplied = true;
                    applied.Add(ev.id);
                    survivors.Add(ev);
                }
                else
                {
                    ev.failureReason = "validation_failed";
                    discardedEvents.Add(ev);
                }
            }

            GameClock.Set(intervalEnd);
            GameTimeManager.UpdateDateFromSeconds(intervalEnd);
            SaveSnapshot(true);

            currentTime = intervalEnd;
            month = nextMonth;
            year = nextYear;
        }

        events.Clear();
        events.AddRange(survivors);
    }

    Snapshot CaptureCurrentStateSnapshot()
    {
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

        var cells = MapState.cellMap.ToDictionary(kv => kv.Key, kv => kv.Value.Clone());
        var res = new Dictionary<string, GameResources>
        {
            ["player"] = CloneResources(GameState.playerResources)
        };

        return new Snapshot
        {
            timestamp = GameClock.Time,
            cellDeltas = cells,
            resourceDeltas = res,
            characters = charList
        };
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

    public List<WorldEvent> GetDiscardedEvents()
    {
        return new List<WorldEvent>(discardedEvents);
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
        foreach (var snap in oldSnaps.Where(s => s.timestamp > cutoff))
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
        // 1. Adaptar celdas
        if (snap.cellDeltas != null)
        {
            var keysToRemove = new List<Vector2Int>();
            foreach (var kv in snap.cellDeltas)
            {
                var pos = kv.Key;
                var snapshotCell = kv.Value;

                if (!MapState.cellMap.TryGetValue(pos, out var currentCell))
                {
                    // Celda eliminada en nuevo pasado
                    keysToRemove.Add(pos);
                    continue;
                }

                if (!string.IsNullOrEmpty(snapshotCell.building) && string.IsNullOrEmpty(currentCell.building))
                {
                    // Edificio existía en la línea vieja, pero fue demolido
                    keysToRemove.Add(pos);
                    continue;
                }

                if (snapshotCell.building != currentCell.building || snapshotCell.level != currentCell.level)
                {
                    // Inconsistencia en tipo o nivel
                    keysToRemove.Add(pos);
                    continue;
                }
            }

            foreach (var key in keysToRemove)
                snap.cellDeltas.Remove(key);
        }

        // 2. Adaptar recursos
        if (snap.resourceDeltas != null)
        {
            var keysToRemove = new List<string>();

            foreach (var kv in snap.resourceDeltas)
            {
                string key = kv.Key;
                GameResources snapshotRes = kv.Value;
                GameResources currentRes = GameState.playerResources;

                bool conflict = false;

                if (key == "player")
                {
                    conflict = snapshotRes.gold > currentRes.gold ||
                               snapshotRes.wood > currentRes.wood ||
                               snapshotRes.food > currentRes.food ||
                               snapshotRes.crono > currentRes.crono ||
                               snapshotRes.science > currentRes.science ||
                               snapshotRes.freeHousing > currentRes.freeHousing ||
                               snapshotRes.academicUnits > currentRes.academicUnits ||
                               snapshotRes.barracksUnits > currentRes.barracksUnits;
                }
                else
                {
                    // otros recursos por clave si existieran
                    conflict = true; // o asumimos inválido si no sabemos cómo comparar
                }

                if (conflict)
                    keysToRemove.Add(key);
            }

            foreach (var key in keysToRemove)
                snap.resourceDeltas.Remove(key);
        }


        // 3. Adaptar personajes
        if (snap.characters != null)
        {
            snap.characters = snap.characters
                .Where(c =>
                {
                    var pos = new Vector2Int(c.x, c.y);

                    // Si la celda no existe o hay un personaje ya ahí, descartamos al duplicado
                    if (!MapState.cellMap.ContainsKey(pos))
                        return false;

                    bool ocupado = GameObject.FindObjectsOfType<Character>()
                        .Any(ch => ch.GetGridPosition() == pos);

                    return !ocupado;
                })
                .ToList();
        }
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
        if (orderedSnapshots.Count == 0)
            return world;

        var first = orderedSnapshots[0];
        if (time < first.timestamp)
        {
            // No se puede viajar antes del inicio: fijamos el tiempo al primer snapshot
            time = first.timestamp;
        }

        foreach (var snap in orderedSnapshots)
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
        GameState.playerResources = world.resources.TryGetValue("player", out var res)
            ? CloneResources(res)
            : new GameResources();

        lastSnapshotCells = world.cells.ToDictionary(kv => kv.Key, kv => kv.Value.Clone());
        lastSnapshotResources = world.resources.ToDictionary(kv => kv.Key, kv => CloneResources(kv.Value));

        MapLoader.instance?.ReloadFromState();

        if (MapLoader.instance != null)
        {
            foreach (var pos in MapState.cellMap.Keys.ToList())
            {
                if (!world.cells.ContainsKey(pos))
                {
                    MapState.cellMap.Remove(pos);
                    MapLoader.instance?.RemoveTileAt(pos); // Necesitás implementar esto si no existe
                }
            }
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
        var targetList = isTimeTraveling ? currentTimelineSnapshots : currentBranch.snapshots;

        if (!force)
        {
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

        if (existingIndex >= 0)
            targetList[existingIndex] = snap;
        else
            targetList.Add(snap);

        // Ensure changes only affect future states by discarding snapshots after this moment
        for (int i = targetList.Count - 1; i >= 0; i--)
        {
            if (targetList[i].timestamp > snap.timestamp)
                targetList.RemoveAt(i);
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

    public List<WorldEvent> GetEvents()
    {
        return new List<WorldEvent>(currentBranch.events);
    }

    public void SetEvents(List<WorldEvent> evs)
    {
        currentBranch.events = evs ?? new List<WorldEvent>();
        if (currentBranch.events.Count > 0)
            nextId = currentBranch.events.Max(e => e.id) + 1;
        else
            nextId = 1;
        RebuildEntityLogs();
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
