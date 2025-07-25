using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using static DTO;

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
                Character w = autoIdleWorkers[i];
                Character.GatherTask task;
                if (i < targetGold) task = Character.GatherTask.Gold;
                else if (i < targetGold + targetWood) task = Character.GatherTask.Wood;
                else task = Character.GatherTask.Food;

                w.gatherTask = task;
                PlanGatherRoute(w);
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

    Vector2Int FindNearest(Vector2Int origin, System.Func<MapCellDTO, bool> filter)
    {
        int best = int.MaxValue;
        Vector2Int bestPos = new(-1, -1);
        foreach (var kvp in MapState.cellMap)
        {
            if (!filter(kvp.Value))
                continue;
            int dist = Mathf.Abs(kvp.Key.x - origin.x) + Mathf.Abs(kvp.Key.y - origin.y);
            if (dist < best)
            {
                best = dist;
                bestPos = kvp.Key;
            }
        }
        return bestPos;
    }

    void PlanGatherRoute(Character worker)
    {
        Vector2Int start = worker.GetGridPosition();
        Vector2Int townhall = FindNearest(start, c => c.building == "townhall");
        if (townhall.x < 0) return;

        List<Vector2Int> route = new();
        List<Vector2Int> segment;
        switch (worker.gatherTask)
        {
            case Character.GatherTask.Gold:
                Vector2Int mine = FindNearest(start, c => c.building == "mine");
                if (mine.x < 0) return;
                segment = Pathfinder.FindPath(start, mine, MapState.cellMap);
                route.AddRange(segment);
                segment = Pathfinder.FindPath(mine, townhall, MapState.cellMap);
                for (int i = 1; i < segment.Count; i++) route.Add(segment[i]);
                segment = Pathfinder.FindPath(townhall, mine, MapState.cellMap);
                for (int i = 1; i < segment.Count; i++) route.Add(segment[i]);
                break;
            case Character.GatherTask.Wood:
                Vector2Int lumber = FindNearest(start, c => c.building == "lumbermill");
                if (lumber.x < 0) return;
                Vector2Int tree = FindNearest(lumber, c => c.resources != null && c.resources.wood > 0);
                if (tree.x < 0) return;
                segment = Pathfinder.FindPath(start, lumber, MapState.cellMap);
                route.AddRange(segment);
                segment = Pathfinder.FindPath(lumber, tree, MapState.cellMap);
                for (int i = 1; i < segment.Count; i++) route.Add(segment[i]);
                segment = Pathfinder.FindPath(tree, lumber, MapState.cellMap);
                for (int i = 1; i < segment.Count; i++) route.Add(segment[i]);
                break;
            case Character.GatherTask.Food:
                Vector2Int farm = FindNearest(start, c => c.building == "farm");
                if (farm.x < 0) return;
                segment = Pathfinder.FindPath(start, farm, MapState.cellMap);
                route.AddRange(segment);
                segment = Pathfinder.FindPath(farm, townhall, MapState.cellMap);
                for (int i = 1; i < segment.Count; i++) route.Add(segment[i]);
                segment = Pathfinder.FindPath(townhall, farm, MapState.cellMap);
                for (int i = 1; i < segment.Count; i++) route.Add(segment[i]);
                break;
        }

        if (route.Count > 0)
            worker.SetGatherRoute(route);
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
