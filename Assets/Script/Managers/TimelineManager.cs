using System.Collections.Generic;
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

    public List<WorldEvent> GetWorldStateAt(float time)
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
        List<WorldEvent> events = new();
        foreach (var e in globalEvents)
        {
            if (e.timestamp > time) break;
            events.Add(e);
        }
        return events;
    }

    public void SaveSnapshot()
    {
        var snap = new Snapshot();
        snap.timestamp = Time.time;
        snap.cells = new Dictionary<Vector2Int, DTO.MapCellDTO>(MapState.cellMap);
        snap.resources = new Dictionary<string, GameResources>();
        // placeholder - deep copy resources if needed
        snapshots.Add(snap);
    }
}
