using UnityEngine;

public class ResourceFlow
{
    public int gold;
    public int wood;
    public int food;
    public int crono;

    public ResourceFlow(int gold = 0, int wood = 0, int food = 0, int crono = 0)
    {
        this.gold = gold;
        this.wood = wood;
        this.food = food;
        this.crono = crono;
    }

    public ResourceFlow Scale(float factor)
    {
        return new ResourceFlow(
            Mathf.RoundToInt(gold * factor),
            Mathf.RoundToInt(wood * factor),
            Mathf.RoundToInt(food * factor),
            Mathf.RoundToInt(crono * factor)
        );
    }

    public static ResourceFlow operator +(ResourceFlow a, ResourceFlow b) =>
        new ResourceFlow(a.gold + b.gold, a.wood + b.wood, a.food + b.food, a.crono + b.crono);
}