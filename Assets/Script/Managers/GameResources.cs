public class GameResources
{
    public int gold = 100;
    public int wood = 100;
    public int food = 100;
    public int crono = 50;
    public int freeHousing = 3;
    public int academicUnits = 0;
    public int barracksUnits = 0;

    public bool HasEnough(BuildRequirement req)
    {
        return gold >= req.gold &&
               wood >= req.wood &&
               food >= req.food &&
               crono >= req.crono &&
               freeHousing >= req.housing &&
               academicUnits >= req.academicUnits &&
               barracksUnits >= req.barracksUnits;
    }

    public void Consume(BuildRequirement req)
    {
        gold -= req.gold;
        wood -= req.wood;
        food -= req.food;
        crono -= req.crono;
        freeHousing -= req.housing;
        academicUnits -= req.academicUnits;
        barracksUnits -= req.barracksUnits;
    }

    public override string ToString()
    {
        return $"Oro: {gold}, Madera: {wood}, Comida: {food}, Crono: {crono}, Habitaciones: {freeHousing}, Ciencia: {academicUnits}";
    }

    public void AddFlow(ResourceFlow flow)
    {
        gold += flow.gold;
        wood += flow.wood;
        food += flow.food;
        crono += flow.crono;
    }

}
