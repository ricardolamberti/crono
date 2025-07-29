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

    public static ResourceFlow GetFlow(string code)
    {
        return production.TryGetValue(code, out var flow) ? flow : new ResourceFlow();
    }

    public static ResourceFlow GetFlow(CharacterRole role)
    {
        return GetFlow(role.Code);
    }
}
