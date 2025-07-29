using System;
using System.Collections.Generic;

public static class BuildingFactory
{
    private static readonly Dictionary<string, Func<Building>> constructors = new()
    {
        { "townhall", () => new TownhallLevel1() },
        { "barracks", () => new BarracksLevel1() },
        { "airport", () => new AirportLevel1() },
        { "dock", () => new DockLevel1() },
        { "hut", () => new HutLevel1() },
        { "farm", () => new FarmLevel1() },
        { "academy", () => new AcademyLevel1() },
        { "atalaya", () => new AtalayaLevel1() },
        { "wall", () => new WallLevel1() },
        { "lumbermill", () => new SawmillLevel1() },
        { "extractor", () => new CronoExtractorLevel1() }
    };

    public static Building Create(string code)
    {
        if (string.IsNullOrEmpty(code))
            return null;
        return constructors.TryGetValue(code, out var ctor) ? ctor() : null;
    }
}
