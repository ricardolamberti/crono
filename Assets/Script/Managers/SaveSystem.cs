using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using static DTO;

[System.Serializable]
public class GameSaveData
{
    public List<MapCellDTO> cells;
    public GameResources resources;
    public float time;
    public int year;
    public int month;
    public List<Vec2IntDTO> explored;
    public List<CharacterDTO> characters;
    public List<SnapshotDTO> snapshots;
    public List<WorldEventDTO> events;
}

[System.Serializable]
public class SnapshotDTO
{
    public float timestamp;
    public List<MapCellDTO> cells;
    public List<SnapshotResourceDTO> resources;
    public List<CharacterDTO> characters;
}

[System.Serializable]
public class SnapshotResourceDTO
{
    public string id;
    public GameResources resources;
}

[System.Serializable]
public class WorldEventDTO
{
    public int id;
    public float timestamp;
    public string actorId;
    public string action;
    public List<WorldEventParamDTO> parameters;
    public List<int> dependencies;
}

[System.Serializable]
public class WorldEventParamDTO
{
    public string key;
    public string value;
}

public static class SaveSystem
{
    private static string SaveFolder => Path.Combine(Application.persistentDataPath, "saves");

    public static void SaveGame()
    {
        Directory.CreateDirectory(SaveFolder);
        GameSaveData data = new GameSaveData
        {
            cells = new List<MapCellDTO>(MapState.cellMap.Values),
            resources = GameState.playerResources,
            time = GameClock.Time,
            year = GameTimeManager.CurrentYear,
            month = GameTimeManager.CurrentMonth,
            explored = new List<Vec2IntDTO>(),
            characters = new List<CharacterDTO>()
        };

        foreach (var pos in MapState.exploredCells)
            data.explored.Add(new Vec2IntDTO(pos.x, pos.y));

        foreach (var character in GameObject.FindObjectsOfType<Character>())
        {
            Vector2Int p = character.GetGridPosition();
            string type = character.characterType.ToString();
            if (character.role != null)
                type = character.role.Code;
            data.characters.Add(new CharacterDTO
            {
                x = p.x,
                y = p.y,
                type = type,
                owner = character.owner
            });
        }
        if (TimelineManager.Instance != null)
        {
            TimelineManager.Instance.SaveSnapshot(true);
            data.snapshots = new List<SnapshotDTO>();
            data.events = new List<WorldEventDTO>();
            foreach (var snap in TimelineManager.Instance.GetSnapshots())
            {
                var sDto = new SnapshotDTO
                {
                    timestamp = snap.timestamp,
                    cells = new List<MapCellDTO>(snap.cellDeltas.Values),
                    resources = new List<SnapshotResourceDTO>(),
                    characters = new List<CharacterDTO>()
                };
                foreach (var kv in snap.resourceDeltas)
                {
                    sDto.resources.Add(new SnapshotResourceDTO { id = kv.Key, resources = kv.Value });
                }
                if (snap.characters != null)
                {
                    foreach (var c in snap.characters)
                        sDto.characters.Add(new CharacterDTO { x = c.x, y = c.y, type = c.type, owner = c.owner });
                }
                data.snapshots.Add(sDto);
            }

            foreach (var ev in TimelineManager.Instance.GetEvents())
            {
                var eDto = new WorldEventDTO
                {
                    id = ev.id,
                    timestamp = ev.timestamp,
                    actorId = ev.actorId,
                    action = ev.action,
                    parameters = new List<WorldEventParamDTO>(),
                    dependencies = new List<int>(ev.dependencies)
                };
                foreach (var kv in ev.parameters)
                {
                    eDto.parameters.Add(new WorldEventParamDTO { key = kv.Key, value = kv.Value });
                }
                data.events.Add(eDto);
            }
        }
        string json = JsonUtility.ToJson(data, true);
        string path = Path.Combine(SaveFolder, DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".json");
        File.WriteAllText(path, json);
        Debug.Log($"Game saved to {path}");
    }

    public static string[] GetSavedFiles()
    {
        if (!Directory.Exists(SaveFolder))
            return Array.Empty<string>();
        return Directory.GetFiles(SaveFolder, "*.json");
    }

    public static void LoadGame(string path)
    {
        if (!File.Exists(path))
        {
            Debug.LogError($"Save not found: {path}");
            return;
        }
        string json = File.ReadAllText(path);
        var data = JsonUtility.FromJson<GameSaveData>(json);
        if (data == null)
        {
            Debug.LogError("Invalid save file");
            return;
        }
        MapState.cellMap = new Dictionary<Vector2Int, MapCellDTO>();
        foreach (var cell in data.cells)
            MapState.cellMap[new Vector2Int(cell.x, cell.y)] = cell;
        GameState.playerResources = data.resources ?? new GameResources();
        GameTimeManager.CurrentYear = data.year;
        GameTimeManager.CurrentMonth = data.month;
        GameClock.Set(data.time);
        GameTimeManager.UpdateDateFromSeconds(GameClock.Time);
        MapState.exploredCells = new HashSet<Vector2Int>();
        if (data.explored != null)
        {
            foreach (var pos in data.explored)
                MapState.exploredCells.Add(new Vector2Int(pos.x, pos.y));
        }
        if (TimelineManager.Instance != null)
        {
            var snaps = new List<Snapshot>();
            if (data.snapshots != null)
            {
                foreach (var sDto in data.snapshots)
                {
                    var snap = new Snapshot
                    {
                        timestamp = sDto.timestamp,
                        cellDeltas = new Dictionary<Vector2Int, MapCellDTO>(),
                        resourceDeltas = new Dictionary<string, GameResources>(),
                        characters = new List<CharacterDTO>()
                    };
                    if (sDto.cells != null)
                    {
                        foreach (var cell in sDto.cells)
                            snap.cellDeltas[new Vector2Int(cell.x, cell.y)] = cell;
                    }
                    if (sDto.resources != null)
                    {
                        foreach (var r in sDto.resources)
                            snap.resourceDeltas[r.id] = r.resources;
                    }
                    if (sDto.characters != null)
                    {
                        foreach (var c in sDto.characters)
                            snap.characters.Add(new CharacterDTO { x = c.x, y = c.y, type = c.type, owner = c.owner });
                    }
                    snaps.Add(snap);
                }
            }
            var events = new List<WorldEvent>();
            if (data.events != null)
            {
                foreach (var eDto in data.events)
                {
                    var ev = new WorldEvent(eDto.id, eDto.timestamp, eDto.actorId, eDto.action);
                    if (eDto.parameters != null)
                    {
                        foreach (var p in eDto.parameters)
                            ev.parameters[p.key] = p.value;
                    }
                    if (eDto.dependencies != null)
                        ev.dependencies.AddRange(eDto.dependencies);
                    events.Add(ev);
                }
                events = events.OrderBy(e => e.timestamp).ToList();
            }
            TimelineManager.Instance.SetEvents(events);
            TimelineManager.Instance.SetSnapshots(snaps);
        }
        Debug.Log($"Game loaded from {path}");

        if (MapLoader.instance != null)
        {
            MapLoader.instance.ReloadFromState();
            foreach (var ch in GameObject.FindObjectsOfType<Character>())
                GameObject.Destroy(ch.gameObject);
            if (data.characters != null)
            {
                foreach (var c in data.characters)
                {
                    Character.Type type;
                    if (System.Enum.TryParse(c.type, out type))
                        MapLoader.instance.SpawnCharacter(new Vector2Int(c.x, c.y), type, c.owner);
                    else
                    {
                        if (c.type == "worker" || c.type == "scientist" || c.type == "warrior")
                        {
                            if (c.type == "worker") type = Character.Type.Worker;
                            else if (c.type == "scientist") type = Character.Type.Scientist;
                            else type = Character.Type.Warrior;
                            MapLoader.instance.SpawnCharacter(new Vector2Int(c.x, c.y), type, c.owner);
                        }
                    }
                }
            }
            foreach (var pos in MapState.exploredCells)
                MapLoader.instance.RevealTile(pos);
        }
    }
}
