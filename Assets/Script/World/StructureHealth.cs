using UnityEngine;

public class StructureHealth : MonoBehaviour
{
    public int maxResistance = 10;
    public int currentResistance;
    public Vector2Int gridPosition;

    void Awake()
    {
        currentResistance = maxResistance;
    }

    public void Initialize(Vector2Int pos, int maxRes)
    {
        gridPosition = pos;
        maxResistance = maxRes;
        currentResistance = maxResistance;
    }

    public void TakeDamage(int amount)
    {
        currentResistance -= amount;
        currentResistance = Mathf.Max(currentResistance, 0);
        Debug.Log($"{name} recibio {amount} de daño. Resistencia actual: {currentResistance}");
        if (currentResistance == 0)
            DestroyStructure();
    }

    void DestroyStructure()
    {
        Debug.Log($"{name} ha sido destruido.");
        if (MapLoader.instance != null)
            MapLoader.instance.DemolishBuilding(gridPosition);
        else
            Destroy(gameObject);
    }
}
