using UnityEngine;

public class TimeBreach : MonoBehaviour
{
    public TimeRequestConfig[] resourceConfigs;
    public TimeRequestConfig workerConfig;
    public TimeRequestConfig[] soldierConfigs;

    private Vector2Int position;
    public Vector2Int Position => position;

    public void Initialize(Vector2Int pos)
    {
        position = pos;
    }

    public int CalculateCost(TimeRequestConfig config, int years)
    {
        return Mathf.RoundToInt(config.baseCost + (years * config.costFactor));
    }

    public void MakeRequest(TimeRequestConfig config, int years)
    {
        int cost = CalculateCost(config, years);
        if (GameState.playerResources.crono < cost)
        {
            Debug.Log("No hay crono suficiente");
            return;
        }

        GameState.playerResources.crono -= cost;
        TimeRequestsManager.Instance.RegisterRequest(this, config, years);
        // Placeholder visual effect
        Debug.Log($"Pedido {config.id} en {years} años");
    }
}
