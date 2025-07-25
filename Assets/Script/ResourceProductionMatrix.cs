using System.Collections.Generic;
using UnityEngine;

public static class ResourceProductionMatrix
{
    public static Dictionary<string, ResourceFlow> production = new()
    {
        { "farm", new ResourceFlow(food: +5) },
        { "mine", new ResourceFlow(gold: +3) },
        { "lumbermill", new ResourceFlow(wood: +4) },
        { "crono_extractor", new ResourceFlow(crono: +2) },
        { "worker", new ResourceFlow(food: -1) },
        { "scientist", new ResourceFlow(food: -1) },
        { "warrior", new ResourceFlow(food: -2) },
    };

    public static ResourceFlow GetFlow(string code)
    {
        return production.TryGetValue(code, out var flow) ? flow : new ResourceFlow();
    }
}
