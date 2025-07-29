using System;
using System.Collections.Generic;

public static class BuildingFactory
{
    private static readonly Dictionary<(string, int), Func<Building>> constructors = new()
    {
        { ("townhall", 1), () => new TownhallLevel1() },
        { ("townhall", 2), () => new TownhallLevel2() },
        { ("townhall", 3), () => new TownhallLevel3() },
        { ("townhall", 4), () => new TownhallLevel4() },

        { ("barracks", 1), () => new BarracksLevel1() },
        { ("barracks", 2), () => new BarracksLevel2() },
        { ("barracks", 3), () => new BarracksLevel3() },
        { ("barracks", 4), () => new BarracksLevel4() },

        { ("airport", 1), () => new AirportLevel1() },
        { ("airport", 2), () => new AirportLevel2() },

        { ("dock", 1), () => new DockLevel1() },
        { ("dock", 2), () => new DockLevel2() },
        { ("dock", 3), () => new DockLevel3() },
        { ("dock", 4), () => new DockLevel4() },

        { ("hut", 1), () => new HutLevel1() },
        { ("hut", 2), () => new HutLevel2() },
        { ("hut", 3), () => new HutLevel3() },
        { ("hut", 4), () => new HutLevel4() },

        { ("farm", 1), () => new FarmLevel1() },
        { ("farm", 2), () => new FarmLevel2() },
        { ("farm", 3), () => new FarmLevel3() },
        { ("farm", 4), () => new FarmLevel4() },

        { ("academy", 1), () => new AcademyLevel1() },
        { ("academy", 2), () => new AcademyLevel2() },
        { ("academy", 3), () => new AcademyLevel3() },
        { ("academy", 4), () => new AcademyLevel4() },

        { ("atalaya", 1), () => new AtalayaLevel1() },
        { ("atalaya", 2), () => new AtalayaLevel2() },
        { ("atalaya", 3), () => new AtalayaLevel3() },
        { ("atalaya", 4), () => new AtalayaLevel4() },

        { ("wall", 1), () => new WallLevel1() },
        { ("wall", 2), () => new WallLevel2() },
        { ("wall", 3), () => new WallLevel3() },
        { ("wall", 4), () => new WallLevel4() },

        { ("lumbermill", 1), () => new SawmillLevel1() },
        { ("lumbermill", 2), () => new SawmillLevel2() },
        { ("lumbermill", 3), () => new SawmillLevel3() },

        { ("extractor", 1), () => new CronoExtractorLevel1() },
        { ("extractor", 2), () => new CronoExtractorLevel2() },
        { ("extractor", 3), () => new CronoExtractorLevel3() },
        { ("extractor", 4), () => new CronoExtractorLevel4() },
    };

    public static Building Create(string code, int level = 1)
    {
        if (string.IsNullOrEmpty(code))
            return null;

        return constructors.TryGetValue((code, level), out var ctor) ? ctor() : null;
    }
}
