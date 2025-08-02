using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using static DTO;
using GameConstants;

public class GameTimeManager : MonoBehaviour
{
    public static GameTimeManager Instance { get; private set; }
    public float interval = 5f; // cada 5 segundos
    public int cyclesPerMonth = 30;

    private const int workersPerBuilding = 3;

    private float timer = 0f;
    private int currentCycle = 0;

    public static int CurrentMonth { get;  set; } = 1;
    public static int CurrentYear { get;  set; } = 1;

    public static event System.Action<int, int> OnDateChanged;

    private bool observationMode = false;

    void Awake()
    {
        Instance = this;
    }

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
        float farmMultiplier = 0f;
        float lumberMultiplier = 0f;

        foreach (var cell in MapState.cellMap.Values)
        {
            if (string.IsNullOrEmpty(cell.building))
                continue;

            switch (cell.building)
            {
                case BuildingCodes.Farm:
                    farmCount++;
                    farmMultiplier += ResourceProductionMatrix.GetMultiplier(BuildingCodes.Farm, cell.level);
                    break;
                case BuildingCodes.Mine:
                    mineCount++;
                    break;
                case BuildingCodes.Lumbermill:
                    lumberCount++;
                    lumberMultiplier += ResourceProductionMatrix.GetMultiplier(BuildingCodes.Lumbermill, cell.level);
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
            if (character.role != null)
                total += ResourceProductionMatrix.GetFlow(character.role);
            else
                total += ResourceProductionMatrix.GetFlow(character.characterType.ToString().ToLower());

            if (character.role is WorkerRole)
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
                Character w = autoIdleWorkers[i];
                Character.GatherTask task;
                if (i < targetGold) task = Character.GatherTask.Gold;
                else if (i < targetGold + targetWood) task = Character.GatherTask.Wood;
                else task = Character.GatherTask.Food;

                w.gatherTask = task;
                w.PlanGatherRoute();
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
            total += ResourceProductionMatrix.GetFlow(BuildingCodes.Mine).Scale(groups);
        }

        if (lumberCount > 0)
        {
            int effective = Mathf.Min(woodWorkers, lumberCount * workersPerBuilding);
            float groups = effective / (float)workersPerBuilding;
            float avgMultiplier = lumberMultiplier / lumberCount;
            total += ResourceProductionMatrix.GetFlow(BuildingCodes.Lumbermill).Scale(groups * avgMultiplier);
        }

        if (farmCount > 0)
        {
            int effective = Mathf.Min(foodWorkers, farmCount * workersPerBuilding);
            float groups = effective / (float)workersPerBuilding;
            float avgMultiplier = farmMultiplier / farmCount;
            total += ResourceProductionMatrix.GetFlow(BuildingCodes.Farm).Scale(groups * avgMultiplier);
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

    public bool Approve(ControlPanelAction action)
    {
        return !observationMode;
    }

    public bool Approve(InfoItem info)
    {
        return !observationMode;
    }

    public void SetObservationMode(bool value)
    {
        observationMode = value;
    }
}
