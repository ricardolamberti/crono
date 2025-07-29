using System.Collections.Generic;

public static class BuildingEvolutionMatrix
{
    public class Evolution
    {
        public string next;
        public int level;
        public int requiredScience;

        public Evolution(string next, int level, int science)
        {
            this.next = next;
            this.level = level;
            this.requiredScience = science;
        }
    }

    public static readonly Dictionary<(string, int), Evolution> evolutions = new()
    {

        // Townhall
        { ("townhall", 1), new Evolution("townhall", 2, 10) },
        { ("townhall", 2), new Evolution("townhall", 3, 20) },
        { ("townhall", 3), new Evolution("townhall", 4, 30) },

        // Barracks
        { ("barracks", 1), new Evolution("barracks", 2, 10) },
        { ("barracks", 2), new Evolution("barracks", 3, 20) },
        { ("barracks", 3), new Evolution("barracks", 4, 30) },

        // Airport
        { ("airport", 1), new Evolution("airport", 2, 10) },

        // Dock
        { ("dock", 1), new Evolution("dock", 2, 10) },
        { ("dock", 2), new Evolution("dock", 3, 20) },
        { ("dock", 3), new Evolution("dock", 4, 30) },

        // Hut
        { ("hut", 1), new Evolution("hut", 2, 5) },
        { ("hut", 2), new Evolution("hut", 3, 10) },
        { ("hut", 3), new Evolution("hut", 4, 15) },

        // Farm
        { ("farm", 1), new Evolution("farm", 2, 10) },
        { ("farm", 2), new Evolution("farm", 3, 20) },
        { ("farm", 3), new Evolution("farm", 4, 30) },

        // Academy
        { ("academy", 1), new Evolution("academy", 2, 10) },
        { ("academy", 2), new Evolution("academy", 3, 20) },
        { ("academy", 3), new Evolution("academy", 4, 30) },

        // Atalaya
        { ("atalaya", 1), new Evolution("atalaya", 2, 10) },
        { ("atalaya", 2), new Evolution("atalaya", 3, 20) },
        { ("atalaya", 3), new Evolution("atalaya", 4, 30) },

        // Wall
        { ("wall", 1), new Evolution("wall", 2, 10) },
        { ("wall", 2), new Evolution("wall", 3, 20) },
        { ("wall", 3), new Evolution("wall", 4, 30) },

        // Lumbermill
        { ("lumbermill", 1), new Evolution("lumbermill", 2, 10) },
        { ("lumbermill", 2), new Evolution("lumbermill", 3, 20) },

        // Crono Extractor
        { ("extractor", 1), new Evolution("extractor", 2, 10) },
        { ("extractor", 2), new Evolution("extractor", 3, 20) },
        { ("extractor", 3), new Evolution("extractor", 4, 30) },

        // Mine
        { ("mine", 1), new Evolution("mine", 2, 10) },
        { ("mine", 2), new Evolution("mine", 3, 20) },
        { ("mine", 3), new Evolution("mine", 4, 30) }
    };

    public static bool TryGetEvolution(string building, int level, out Evolution evo)
    {
        return evolutions.TryGetValue((building, level), out evo);
    }
}
