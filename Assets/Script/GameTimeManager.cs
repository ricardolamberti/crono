using UnityEngine;

public class GameTimeManager : MonoBehaviour
{
    public float interval = 5f; // cada 5 segundos
    private float timer = 0f;

    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= interval)
        {
            ApplyResourceFlow();
            timer = 0f;
        }
    }

    void ApplyResourceFlow()
    {
        var total = new ResourceFlow();

        foreach (var cell in MapState.cellMap.Values)
        {
            if (!string.IsNullOrEmpty(cell.building))
            {
                total += ResourceProductionMatrix.GetFlow(cell.building);
            }
        }

        foreach (var character in GameObject.FindObjectsOfType<Character>())
        {
            string unitCode = character.characterType.ToString().ToLower();
            total += ResourceProductionMatrix.GetFlow(unitCode);
        }

        GameState.playerResources.AddFlow(total);
    }
}
