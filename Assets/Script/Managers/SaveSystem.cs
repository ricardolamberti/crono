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
            month = GameTimeManager.CurrentMonth
        };
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
        Debug.Log($"Game loaded from {path}");

        if (MapLoader.instance != null)
            MapLoader.instance.ReloadFromState();
    }
}
