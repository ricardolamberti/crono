using System.Collections.Generic;

public static class BuildingEvolutionMatrix
{
    public class Evolution
    {
        public string next;
        public int  level;
        public int requiredScience;
        public Evolution(string next, int level, int science)
        {
            this.next = next;
            this.level = level;
            this.requiredScience = science;
        }
    }

    public static readonly Dictionary<string, Evolution> evolutions = new()
    {
        { "hut", new Evolution("hut",2, 10) },
        { "mine", new Evolution("mine",2, 15) }
    };

    public static bool TryGetEvolution(string building, out Evolution evo)
    {
        return evolutions.TryGetValue(building, out evo);
    }
}
