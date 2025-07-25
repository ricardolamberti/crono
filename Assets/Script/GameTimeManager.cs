using UnityEngine;

public class GameTimeManager : MonoBehaviour
{
    public float interval = 5f; // cada 5 segundos
    public int cyclesPerMonth = 30;

    private const int workersPerBuilding = 3;

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

        List<Character> allWorkers = new();
        List<Character> autoIdleWorkers = new();

        foreach (var character in GameObject.FindObjectsOfType<Character>())
        {
            string unitCode = character.characterType.ToString().ToLower();
            total += ResourceProductionMatrix.GetFlow(unitCode);

            if (character.characterType == Character.Type.Worker)
            {
                if (character.currentTask == Character.Task.None)
                {
                    allWorkers.Add(character);
                    if (character.controlMode == Character.ControlMode.Automatic)
                        autoIdleWorkers.Add(character);
                }
            }
        }

        int totalWeight = farmCount + mineCount + lumberCount;
        if (totalWeight > 0 && autoIdleWorkers.Count > 0)
        {
            int available = autoIdleWorkers.Count;

            int targetGold = Mathf.RoundToInt(available * (mineCount / (float)totalWeight));
            int targetWood = Mathf.RoundToInt(available * (lumberCount / (float)totalWeight));
            int targetFood = available - targetGold - targetWood;

            targetGold = Mathf.Min(targetGold, mineCount * workersPerBuilding);
            targetWood = Mathf.Min(targetWood, lumberCount * workersPerBuilding);
            targetFood = Mathf.Min(targetFood, farmCount * workersPerBuilding);

            int used = Mathf.Min(targetGold + targetWood + targetFood, available);
            int remaining = available - used;

            while (remaining > 0)
            {
                if (targetGold < mineCount * workersPerBuilding)
                { targetGold++; remaining--; if (remaining == 0) break; }
                if (targetWood < lumberCount * workersPerBuilding)
                { targetWood++; remaining--; if (remaining == 0) break; }
                if (targetFood < farmCount * workersPerBuilding)
                { targetFood++; remaining--; if (remaining == 0) break; }
                if (remaining > 0) { targetFood++; remaining--; }
            }

            for (int i = 0; i < autoIdleWorkers.Count; i++)
            {
                if (i < targetGold) autoIdleWorkers[i].gatherTask = Character.GatherTask.Gold;
                else if (i < targetGold + targetWood) autoIdleWorkers[i].gatherTask = Character.GatherTask.Wood;
                else autoIdleWorkers[i].gatherTask = Character.GatherTask.Food;
            }
        }

        int goldWorkers = 0;
        int woodWorkers = 0;
        int foodWorkers = 0;

        foreach (var w in allWorkers)
        {
            switch (w.gatherTask)
            {
                case Character.GatherTask.Gold: goldWorkers++; break;
                case Character.GatherTask.Wood: woodWorkers++; break;
                case Character.GatherTask.Food: foodWorkers++; break;
            }
        }

        if (mineCount > 0)
        {
            int effective = Mathf.Min(goldWorkers, mineCount * workersPerBuilding);
            float groups = effective / (float)workersPerBuilding;
            total += ResourceProductionMatrix.GetFlow("mine").Scale(groups);
        }

        if (lumberCount > 0)
        {
            int effective = Mathf.Min(woodWorkers, lumberCount * workersPerBuilding);
            float groups = effective / (float)workersPerBuilding;
            total += ResourceProductionMatrix.GetFlow("lumbermill").Scale(groups);
        }

        if (farmCount > 0)
        {
            int effective = Mathf.Min(foodWorkers, farmCount * workersPerBuilding);
            float groups = effective / (float)workersPerBuilding;
            total += ResourceProductionMatrix.GetFlow("farm").Scale(groups);
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
