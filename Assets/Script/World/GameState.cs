using System.Collections.Generic;
using UnityEngine;

public static class GameState
{
    public static GameResources playerResources = new GameResources();

    public static Dictionary<string, int> buildingCounts = new();

    public static void IncrementBuilding(string code)
    {
        if (string.IsNullOrEmpty(code))
            return;

        if (!buildingCounts.ContainsKey(code))
            buildingCounts[code] = 0;

        buildingCounts[code]++;
    }

    public static void DecrementBuilding(string code)
    {
        if (string.IsNullOrEmpty(code))
            return;

        if (buildingCounts.ContainsKey(code))
            buildingCounts[code] = Mathf.Max(0, buildingCounts[code] - 1);
    }
}
