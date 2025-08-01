using UnityEngine;

public class TimeBreach : MonoBehaviour
{
    public TimeRequestConfig[] resourceConfigs;
    public TimeRequestConfig workerConfig;
    public TimeRequestConfig[] soldierConfigs;

    void Awake()
    {
        // Ensure default configs exist so the UI does not crash when
        // the component is added dynamically by MapLoader.
        if (resourceConfigs == null || resourceConfigs.Length == 0)
        {
            resourceConfigs = new[]
            {
                CreateConfig("gold"),
                CreateConfig("wood"),
                CreateConfig("food")
            };
        }

        if (workerConfig == null)
        {
            workerConfig = CreateConfig("worker");
        }

        if (soldierConfigs == null || soldierConfigs.Length == 0)
        {
            soldierConfigs = new[] { CreateConfig("warrior") };
        }
    }

    static TimeRequestConfig CreateConfig(string id)
    {
        var cfg = ScriptableObject.CreateInstance<TimeRequestConfig>();
        cfg.id = id;
        cfg.minFutureYears = 0;
        cfg.maxFutureYears = 5;
        cfg.baseCost = 1;
        cfg.costFactor = 1f;
        return cfg;
    }

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
