using UnityEngine;

public class GameTimeManager : MonoBehaviour
{
    public float interval = 5f; // cada 5 segundos
    public int cyclesPerMonth = 30;

    private float timer = 0f;
    private int currentCycle = 0;

    public static int CurrentMonth { get; private set; } = 1;
    public static int CurrentYear { get; private set; } = 1;

    public static event System.Action<int, int> OnDateChanged;

    void Start()
    {
        OnDateChanged?.Invoke(CurrentMonth, CurrentYear);
    }

    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= interval)
        {
            ApplyResourceFlow();
            AdvanceCycle();
            timer = 0f;
        }
    }

    void ApplyResourceFlow()
    {
        var total = new ResourceFlow();

        int farmCount = 0;
        int mineCount = 0;
        int lumberCount = 0;

        foreach (var cell in MapState.cellMap.Values)
        {
            if (string.IsNullOrEmpty(cell.building))
                continue;

            switch (cell.building)
            {
                case "farm":
                    farmCount++;
                    break;
                case "mine":
                    mineCount++;
                    break;
                case "lumbermill":
                    lumberCount++;
                    break;
                default:
                    total += ResourceProductionMatrix.GetFlow(cell.building);
                    break;
            }
        }

        int availableWorkers = 0;

        foreach (var character in GameObject.FindObjectsOfType<Character>())
        {
            string unitCode = character.characterType.ToString().ToLower();
            total += ResourceProductionMatrix.GetFlow(unitCode);

            if (character.characterType == Character.Type.Worker &&
                character.controlMode == Character.ControlMode.Automatic &&
                character.currentTask == Character.Task.None)
            {
                availableWorkers++;
            }
        }

        int totalWeight = farmCount + mineCount + lumberCount;
        float goldWorkers = 0f;
        float woodWorkers = 0f;
        float foodWorkers = 0f;

        if (totalWeight > 0 && availableWorkers > 0)
        {
            goldWorkers = availableWorkers * (mineCount / (float)totalWeight);
            woodWorkers = availableWorkers * (lumberCount / (float)totalWeight);
            foodWorkers = availableWorkers * (farmCount / (float)totalWeight);

            goldWorkers = Mathf.Min(goldWorkers, mineCount);
            woodWorkers = Mathf.Min(woodWorkers, lumberCount);
            foodWorkers = Mathf.Min(foodWorkers, farmCount);

            var mineFlow = ResourceProductionMatrix.GetFlow("mine").Scale(goldWorkers);
            var lumberFlow = ResourceProductionMatrix.GetFlow("lumbermill").Scale(woodWorkers);
            var farmFlow = ResourceProductionMatrix.GetFlow("farm").Scale(foodWorkers);

            total += mineFlow + lumberFlow + farmFlow;
        }

        GameState.playerResources.AddFlow(total);
    }

    void AdvanceCycle()
    {
        currentCycle++;
        if (currentCycle >= cyclesPerMonth)
        {
            currentCycle = 0;
            CurrentMonth++;
            if (CurrentMonth > 12)
            {
                CurrentMonth = 1;
                CurrentYear++;
            }

            OnDateChanged?.Invoke(CurrentMonth, CurrentYear);
        }
    }
}
