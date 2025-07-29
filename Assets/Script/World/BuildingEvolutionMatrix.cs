using System.Collections.Generic;

public static class BuildingEvolutionMatrix
{
    public class Evolution
    {
        public string next;
        public int requiredScience;
        public Evolution(string next, int science)
        {
            this.next = next;
            this.requiredScience = science;
        }
    }

    public static readonly Dictionary<string, Evolution> evolutions = new()
    {
        { "hut", new Evolution("house", 10) },
        { "mine", new Evolution("advanced_mine", 15) }
    };

    public static bool TryGetEvolution(string building, out Evolution evo)
    {
        return evolutions.TryGetValue(building, out evo);
    }
}
