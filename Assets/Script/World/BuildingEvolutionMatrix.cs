using System.Collections.Generic;
using GameConstants;

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
        { (BuildingCodes.Townhall, 1), new Evolution(BuildingCodes.Townhall, 2, 10) },
        { (BuildingCodes.Townhall, 2), new Evolution(BuildingCodes.Townhall, 3, 20) },
        { (BuildingCodes.Townhall, 3), new Evolution(BuildingCodes.Townhall, 4, 30) },

        // Barracks
        { (BuildingCodes.Barracks, 1), new Evolution(BuildingCodes.Barracks, 2, 10) },
        { (BuildingCodes.Barracks, 2), new Evolution(BuildingCodes.Barracks, 3, 20) },
        { (BuildingCodes.Barracks, 3), new Evolution(BuildingCodes.Barracks, 4, 30) },

        // Airport
        { (BuildingCodes.Airport, 1), new Evolution(BuildingCodes.Airport, 2, 10) },

        // Dock
        { (BuildingCodes.Dock, 1), new Evolution(BuildingCodes.Dock, 2, 10) },
        { (BuildingCodes.Dock, 2), new Evolution(BuildingCodes.Dock, 3, 20) },
        { (BuildingCodes.Dock, 3), new Evolution(BuildingCodes.Dock, 4, 30) },

        // Hut
        { (BuildingCodes.Hut, 1), new Evolution(BuildingCodes.Hut, 2, 5) },
        { (BuildingCodes.Hut, 2), new Evolution(BuildingCodes.Hut, 3, 10) },
        { (BuildingCodes.Hut, 3), new Evolution(BuildingCodes.Hut, 4, 15) },

        // Farm
        { (BuildingCodes.Farm, 1), new Evolution(BuildingCodes.Farm, 2, 10) },
        { (BuildingCodes.Farm, 2), new Evolution(BuildingCodes.Farm, 3, 20) },
        { (BuildingCodes.Farm, 3), new Evolution(BuildingCodes.Farm, 4, 30) },

        // Academy
        { (BuildingCodes.Academy, 1), new Evolution(BuildingCodes.Academy, 2, 10) },
        { (BuildingCodes.Academy, 2), new Evolution(BuildingCodes.Academy, 3, 20) },
        { (BuildingCodes.Academy, 3), new Evolution(BuildingCodes.Academy, 4, 30) },

        // Atalaya
        { (BuildingCodes.Atalaya, 1), new Evolution(BuildingCodes.Atalaya, 2, 10) },
        { (BuildingCodes.Atalaya, 2), new Evolution(BuildingCodes.Atalaya, 3, 20) },
        { (BuildingCodes.Atalaya, 3), new Evolution(BuildingCodes.Atalaya, 4, 30) },

        // Wall
        { (BuildingCodes.Wall, 1), new Evolution(BuildingCodes.Wall, 2, 10) },
        { (BuildingCodes.Wall, 2), new Evolution(BuildingCodes.Wall, 3, 20) },
        { (BuildingCodes.Wall, 3), new Evolution(BuildingCodes.Wall, 4, 30) },

        // Lumbermill
        { (BuildingCodes.Lumbermill, 1), new Evolution(BuildingCodes.Lumbermill, 2, 10) },
        { (BuildingCodes.Lumbermill, 2), new Evolution(BuildingCodes.Lumbermill, 3, 20) },

        // Crono Extractor
        { (BuildingCodes.Extractor, 1), new Evolution(BuildingCodes.Extractor, 2, 10) },
        { (BuildingCodes.Extractor, 2), new Evolution(BuildingCodes.Extractor, 3, 20) },
        { (BuildingCodes.Extractor, 3), new Evolution(BuildingCodes.Extractor, 4, 30) },

        // Mine
        { (BuildingCodes.Mine, 1), new Evolution(BuildingCodes.Mine, 2, 10) },
        { (BuildingCodes.Mine, 2), new Evolution(BuildingCodes.Mine, 3, 20) },
        { (BuildingCodes.Mine, 3), new Evolution(BuildingCodes.Mine, 4, 30) }
    };

    public static bool TryGetEvolution(string building, int level, out Evolution evo)
    {
        return evolutions.TryGetValue((building, level), out evo);
    }
}
