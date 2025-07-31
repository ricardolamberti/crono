using System;
using System.Collections.Generic;
using System.Linq;
using GameConstants;

public static class BuildingFactory
{
    private static readonly Dictionary<(string, int), Func<Building>> constructors = new()
    {
        { (GameConstants.BuildingCodes.Townhall, 1), () => new TownhallLevel1() },
        { (GameConstants.BuildingCodes.Townhall, 2), () => new TownhallLevel2() },
        { (GameConstants.BuildingCodes.Townhall, 3), () => new TownhallLevel3() },
        { (GameConstants.BuildingCodes.Townhall, 4), () => new TownhallLevel4() },

        { (GameConstants.BuildingCodes.Barracks, 1), () => new BarracksLevel1() },
        { (GameConstants.BuildingCodes.Barracks, 2), () => new BarracksLevel2() },
        { (GameConstants.BuildingCodes.Barracks, 3), () => new BarracksLevel3() },
        { (GameConstants.BuildingCodes.Barracks, 4), () => new BarracksLevel4() },

        { (GameConstants.BuildingCodes.Airport, 1), () => new AirportLevel1() },
        { (GameConstants.BuildingCodes.Airport, 2), () => new AirportLevel2() },
    
        { (GameConstants.BuildingCodes.Mine, 1), () => new MineLevel1() },
        { (GameConstants.BuildingCodes.Mine, 2), () => new MineLevel2() },
        { (GameConstants.BuildingCodes.Mine, 3), () => new MineLevel3() },
        { (GameConstants.BuildingCodes.Mine, 4), () => new MineLevel4() },

        { (GameConstants.BuildingCodes.Dock, 1), () => new DockLevel1() },
        { (GameConstants.BuildingCodes.Dock, 2), () => new DockLevel2() },
        { (GameConstants.BuildingCodes.Dock, 3), () => new DockLevel3() },
        { (GameConstants.BuildingCodes.Dock, 4), () => new DockLevel4() },

        { (GameConstants.BuildingCodes.Hut, 1), () => new HutLevel1() },
        { (GameConstants.BuildingCodes.Hut, 2), () => new HutLevel2() },
        { (GameConstants.BuildingCodes.Hut, 3), () => new HutLevel3() },
        { (GameConstants.BuildingCodes.Hut, 4), () => new HutLevel4() },

        { (GameConstants.BuildingCodes.Farm, 1), () => new FarmLevel1() },
        { (GameConstants.BuildingCodes.Farm, 2), () => new FarmLevel2() },
        { (GameConstants.BuildingCodes.Farm, 3), () => new FarmLevel3() },
        { (GameConstants.BuildingCodes.Farm, 4), () => new FarmLevel4() },

        { (GameConstants.BuildingCodes.Academy, 1), () => new AcademyLevel1() },
        { (GameConstants.BuildingCodes.Academy, 2), () => new AcademyLevel2() },
        { (GameConstants.BuildingCodes.Academy, 3), () => new AcademyLevel3() },
        { (GameConstants.BuildingCodes.Academy, 4), () => new AcademyLevel4() },

        { (GameConstants.BuildingCodes.Atalaya, 1), () => new AtalayaLevel1() },
        { (GameConstants.BuildingCodes.Atalaya, 2), () => new AtalayaLevel2() },
        { (GameConstants.BuildingCodes.Atalaya, 3), () => new AtalayaLevel3() },
        { (GameConstants.BuildingCodes.Atalaya, 4), () => new AtalayaLevel4() },

        { (GameConstants.BuildingCodes.Wall, 1), () => new WallLevel1() },
        { (GameConstants.BuildingCodes.Wall, 2), () => new WallLevel2() },
        { (GameConstants.BuildingCodes.Wall, 3), () => new WallLevel3() },
        { (GameConstants.BuildingCodes.Wall, 4), () => new WallLevel4() },

        { (GameConstants.BuildingCodes.Lumbermill, 1), () => new SawmillLevel1() },
        { (GameConstants.BuildingCodes.Lumbermill, 2), () => new SawmillLevel2() },
        { (GameConstants.BuildingCodes.Lumbermill, 3), () => new SawmillLevel3() },
        { (GameConstants.BuildingCodes.Lumbermill, 4), () => new SawmillLevel4() },

        { (GameConstants.BuildingCodes.Extractor, 1), () => new CronoExtractorLevel1() },
        { (GameConstants.BuildingCodes.Extractor, 2), () => new CronoExtractorLevel2() },
        { (GameConstants.BuildingCodes.Extractor, 3), () => new CronoExtractorLevel3() },
        { (GameConstants.BuildingCodes.Extractor, 4), () => new CronoExtractorLevel4() },

        { (GameConstants.BuildingCodes.Bridge, 1), () => new BridgeLevel1() },
        { (GameConstants.BuildingCodes.Bridge, 2), () => new BridgeLevel2() },
        { (GameConstants.BuildingCodes.Bridge, 3), () => new BridgeLevel3() },

        { (GameConstants.BuildingCodes.TemporalBreach, 1), () => new TemporalBreach() },
    };

    public static Building Create(string code, int level = 1)
    {
        if (string.IsNullOrEmpty(code))
            return null;

        return constructors.TryGetValue((code, level), out var ctor) ? ctor() : null;
    }

    public static IEnumerable<string> GetAvailableCodes()
    {
        return constructors.Keys.Select(k => k.Item1).Distinct();
    }
}
