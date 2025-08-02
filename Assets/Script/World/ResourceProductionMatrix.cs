using System.Collections.Generic;
using UnityEngine;
using GameConstants;

public static class ResourceProductionMatrix
{
    public static Dictionary<string, ResourceFlow> production = new()
    {
        { BuildingCodes.Farm, new ResourceFlow(food: +5) },
        { BuildingCodes.Mine, new ResourceFlow(gold: +3) },
        { BuildingCodes.AdvancedMine, new ResourceFlow(gold: +6) },
        { BuildingCodes.Lumbermill, new ResourceFlow(wood: +4) },
        { BuildingCodes.CronoExtractor, new ResourceFlow(crono: +2) },
        { BuildingCodes.House, new ResourceFlow(food: +2) },
        { "worker", new ResourceFlow(food: -1) },
        { "scientist", new ResourceFlow(food: -1, science: +1) },
        { "warrior", new ResourceFlow(food: -2) },
    };

    public static readonly Dictionary<(string, int), float> levelMultipliers = new()
    {
        { (BuildingCodes.Lumbermill, 1), 1f },
        { (BuildingCodes.Lumbermill, 2), 2f },
        { (BuildingCodes.Lumbermill, 3), 3f },
        { (BuildingCodes.Lumbermill, 4), 4f },
        { (BuildingCodes.Farm, 1), 1f },
        { (BuildingCodes.Farm, 2), 2f },
        { (BuildingCodes.Farm, 3), 3f },
        { (BuildingCodes.Farm, 4), 4f },
    };

    public static ResourceFlow GetFlow(string code)
    {
        return production.TryGetValue(code, out var flow) ? flow : new ResourceFlow();
    }

    public static ResourceFlow GetFlow(CharacterRole role)
    {
        return GetFlow(role.Code);
    }

    public static float GetMultiplier(string code, int level)
    {
        return levelMultipliers.TryGetValue((code, level), out var mult) ? mult : 1f;
    }
}
