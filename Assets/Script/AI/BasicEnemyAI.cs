using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class BasicEnemyAI
{
    private readonly string ownerId;
    private float timer = 0f;
    private readonly float decisionInterval = 3f;
    
    private enum BuildPhase { NeedTownhall, NeedMine, Established }
    private BuildPhase currentPhase = BuildPhase.NeedTownhall;

    public BasicEnemyAI(string owner)
    {
        ownerId = owner;
    }

    public void Update()
    {
        timer += Time.deltaTime;
        if (timer >= decisionInterval)
        {
            timer = 0f;
            EvaluateStrategy();
        }
    }

    void EvaluateStrategy()
    {
        UpdateBuildPhase();

        Character[] myCharacters = GetMyCharacters();

        switch (currentPhase)
        {
            case BuildPhase.NeedTownhall:
                BuildTownhall();
                break;
            case BuildPhase.NeedMine:
                BuildGoldMine();
                break;
            case BuildPhase.Established:
                ManageEstablishedBase();
                break;
        }
    }

    Character[] GetMyCharacters()
    {
        return GameObject.FindObjectsOfType<Character>()
            .Where(c => c.owner == ownerId)
            .ToArray();
    }

    void UpdateBuildPhase()
    {
        bool hasTownhall = HasBuilding("townhall");
        bool hasMine = HasBuilding("mine");

        if (!hasTownhall)
            currentPhase = BuildPhase.NeedTownhall;
        else if (!hasMine)
            currentPhase = BuildPhase.NeedMine;
        else
            currentPhase = BuildPhase.Established;
    }

    bool HasBuilding(string buildingType)
    {
        return MapState.cellMap.Values.Any(cell => 
            cell.building == buildingType && cell.owner == ownerId);
    }

    void BuildTownhall()
    {
        var idleWorker = GetMyCharacters().FirstOrDefault(c =>
            c.currentTask == Character.Task.None && c.characterType == Character.Type.Worker);
        
        if (idleWorker == null) return;

        Vector2Int bestLocation = FindOptimalTownhallLocation();
        if (bestLocation.x >= 0)
        {
            Debug.Log($"AI {ownerId}: Construyendo townhall en {bestLocation}");
            idleWorker.AssignBuildTask(bestLocation, "townhall");
        }
    }

    void BuildGoldMine()
    {
        var idleWorker = GetMyCharacters().FirstOrDefault(c =>
            c.currentTask == Character.Task.None && c.characterType == Character.Type.Worker);
        
        if (idleWorker == null) return;

        Vector2Int mineLocation = FindBestMountainForMine();
        if (mineLocation.x >= 0)
        {
            Debug.Log($"AI {ownerId}: Construyendo mina en {mineLocation}");
            idleWorker.AssignBuildTask(mineLocation, "mine");
        }
    }

    void ManageEstablishedBase()
    {
        foreach (var c in GetMyCharacters())
        {
            if (c.currentTask == Character.Task.None)
            {
                c.gatherTask = Character.GatherTask.Gold;
                c.PlanGatherRoute();
            }
        }
    }

    Vector2Int FindOptimalTownhallLocation()
    {
        var forestCells = MapState.cellMap.Where(kvp => 
            kvp.Value.terrain == "forest" && 
            string.IsNullOrEmpty(kvp.Value.building)).ToList();

        Vector2Int bestLocation = new(-1, -1);
        float bestScore = -1f;

        foreach (var cell in forestCells)
        {
            float score = EvaluateTownhallLocation(cell.Key);
            if (score > bestScore)
            {
                bestScore = score;
                bestLocation = cell.Key;
            }
        }

        return bestLocation;
    }

    float EvaluateTownhallLocation(Vector2Int position)
    {
        float score = 0f;
        
        // Buscar oro cercano
        float closestGold = FindClosestResourceDistance(position, "gold");
        if (closestGold < float.MaxValue)
            score += 100f / (1f + closestGold * 0.5f);
        
        // Buscar crono cercano
        float closestCrono = FindClosestResourceDistance(position, "crono");
        if (closestCrono < float.MaxValue)
            score += 50f / (1f + closestCrono * 0.5f);
        
        // Penalizar si está muy cerca del borde
        if (position.x < 2 || position.y < 2 || position.x > 18 || position.y > 18)
            score *= 0.5f;
            
        return score;
    }

    float FindClosestResourceDistance(Vector2Int from, string resource)
    {
        float minDistance = float.MaxValue;
        
        foreach (var cell in MapState.cellMap.Values)
        {
            if (cell.resources == null) continue;
            
            bool hasResource = resource == "gold" ? cell.resources.gold > 0 :
                              resource == "crono" ? cell.resources.crono > 0 :
                              resource == "wood" ? cell.resources.wood > 0 : false;
            
            if (hasResource)
            {
                float distance = Vector2Int.Distance(from, new Vector2Int(cell.x, cell.y));
                minDistance = Mathf.Min(minDistance, distance);
            }
        }
        
        return minDistance;
    }

    Vector2Int FindBestMountainForMine()
    {
        var townhallPos = FindMyTownhallPosition();
        if (townhallPos.x < 0) return new(-1, -1);

        var mountainCells = MapState.cellMap.Where(kvp => 
            kvp.Value.terrain == "mountain" && 
            string.IsNullOrEmpty(kvp.Value.building)).ToList();

        Vector2Int bestLocation = new(-1, -1);
        float bestScore = -1f;

        foreach (var cell in mountainCells)
        {
            float distance = Vector2Int.Distance(cell.Key, townhallPos);
            float score = 100f / (1f + distance * 0.3f);
            
            if (score > bestScore)
            {
                bestScore = score;
                bestLocation = cell.Key;
            }
        }

        return bestLocation;
    }

    Vector2Int FindMyTownhallPosition()
    {
        var townhall = MapState.cellMap.FirstOrDefault(kvp => 
            kvp.Value.building == "townhall" && kvp.Value.owner == ownerId);
        
        return townhall.Key != default ? townhall.Key : new(-1, -1);
    }
}
