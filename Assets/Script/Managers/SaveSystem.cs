using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using static DTO;

[System.Serializable]
public class GameSaveData
{
    public List<MapCellDTO> cells;
    public GameResources resources;
    public int year;
    public int month;
    public List<Vec2IntDTO> explored;
    public List<CharacterDTO> characters;
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
        MapState.exploredCells = new HashSet<Vector2Int>();
        if (data.explored != null)
        {
            foreach (var pos in data.explored)
                MapState.exploredCells.Add(new Vector2Int(pos.x, pos.y));
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
