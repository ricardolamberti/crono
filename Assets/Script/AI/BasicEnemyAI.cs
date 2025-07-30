using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using GameConstants;

public class BasicEnemyAI
{
    private readonly string ownerId;
    private readonly Vector2Int spawnPosition;
    private float timer = 0f;
    private readonly float decisionInterval = 3f;
    
    private enum BuildPhase
    {
        NeedTownhall,
        NeedMine,
        NeedFarm,
        NeedLumbermill,
        NeedHut,
        NeedBarracks,
        NeedSoldiers,
        NeedTowers,
        NeedAcademy,
        NeedExtraHut,
        Established
    }

    private BuildPhase currentPhase = BuildPhase.NeedTownhall;

    public BasicEnemyAI(string owner, Vector2Int spawn)
    {
        ownerId = owner;
        spawnPosition = spawn;
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
            case BuildPhase.NeedFarm:
                BuildFarm();
                break;
            case BuildPhase.NeedLumbermill:
                BuildLumbermill();
                break;
            case BuildPhase.NeedHut:
                BuildHut();
                break;
            case BuildPhase.NeedBarracks:
                BuildBarracks();
                break;
            case BuildPhase.NeedSoldiers:
                RecruitSoldiers();
                break;
            case BuildPhase.NeedTowers:
                BuildAtalaya();
                break;
            case BuildPhase.NeedAcademy:
                BuildAcademy();
                break;
            case BuildPhase.NeedExtraHut:
                BuildHut();
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
        bool hasTownhall = HasBuilding(BuildingCodes.Townhall);
        bool hasMine = HasBuilding(BuildingCodes.Mine);
        bool hasFarm = HasBuilding(BuildingCodes.Farm);
        bool hasLumber = HasBuilding(BuildingCodes.Lumbermill);
        bool hasHut = HasBuilding(BuildingCodes.Hut);
        bool hasBarracks = HasBuilding(BuildingCodes.Barracks);
        bool hasAcademy = HasBuilding(BuildingCodes.Academy);
        int warriorCount = CountMyWarriors();
        int towerCount = CountBuildings(BuildingCodes.Atalaya);
        int hutCount = CountBuildings(BuildingCodes.Hut);

        if (!hasTownhall)
            currentPhase = BuildPhase.NeedTownhall;
        else if (!hasMine)
            currentPhase = BuildPhase.NeedMine;
        else if (!hasFarm)
            currentPhase = BuildPhase.NeedFarm;
        else if (!hasLumber)
            currentPhase = BuildPhase.NeedLumbermill;
        else if (!hasHut)
            currentPhase = BuildPhase.NeedHut;
        else if (!hasBarracks)
            currentPhase = BuildPhase.NeedBarracks;
        else if (warriorCount < 10)
            currentPhase = BuildPhase.NeedSoldiers;
        else if (towerCount < 2)
            currentPhase = BuildPhase.NeedTowers;
        else if (!hasAcademy)
            currentPhase = BuildPhase.NeedAcademy;
        else if (hutCount < 2)
            currentPhase = BuildPhase.NeedExtraHut;
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
            idleWorker.AssignBuildTask(bestLocation, BuildingCodes.Townhall);
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
            idleWorker.AssignBuildTask(mineLocation, BuildingCodes.Mine);
        }
    }

    void ManageEstablishedBase()
    {
        var workers = GetMyCharacters()
            .Where(c => c.characterType == Character.Type.Worker)
            .ToArray();
        int index = 0;
        foreach (var w in workers)
        {
            if (w.currentTask != Character.Task.None) continue;

            switch (index % 3)
            {
                case 0: w.gatherTask = Character.GatherTask.Gold; break;
                case 1: w.gatherTask = Character.GatherTask.Wood; break;
                default: w.gatherTask = Character.GatherTask.Food; break;
            }

            w.PlanGatherRoute();
            index++;
        }
    }

    Vector2Int FindOptimalTownhallLocation()
    {
        var forestCells = MapState.cellMap.Where(kvp =>
            kvp.Value.terrain == TerrainTypes.Forest &&
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

        // Preferir proximidad al punto de inicio del jugador
        float spawnDist = Vector2Int.Distance(spawnPosition, position);
        score += 50f / (1f + spawnDist * 0.5f);
        
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
            kvp.Value.terrain == TerrainTypes.Mountain &&
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
            kvp.Value.building == BuildingCodes.Townhall && kvp.Value.owner == ownerId);

        return townhall.Key != default ? townhall.Key : new(-1, -1);
    }

    int CountMyWarriors()
    {
        return GetMyCharacters().Count(c => c.characterType == Character.Type.Warrior);
    }

    int CountBuildings(string code)
    {
        return MapState.cellMap.Values.Count(c => c.building == code && c.owner == ownerId);
    }

    Character GetIdleWorker()
    {
        return GetMyCharacters().FirstOrDefault(c =>
            c.currentTask == Character.Task.None && c.characterType == Character.Type.Worker);
    }

    Vector2Int FindNearestEmptyCell(Vector2Int origin, string terrain)
    {
        int best = int.MaxValue;
        Vector2Int bestPos = new(-1, -1);
        foreach (var kvp in MapState.cellMap)
        {
            if (kvp.Value.terrain != terrain) continue;
            if (!string.IsNullOrEmpty(kvp.Value.building)) continue;

            int dist = Mathf.Abs(kvp.Key.x - origin.x) + Mathf.Abs(kvp.Key.y - origin.y);
            if (dist < best)
            {
                best = dist;
                bestPos = kvp.Key;
            }
        }
        return bestPos;
    }

    Vector2Int FindAdjacentFreeCell(Vector2Int basePos)
    {
        foreach (var dir in new[] { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right })
        {
            Vector2Int target = basePos + dir;
            if (MapLoader.instance != null && MapLoader.instance.IsPositionFree(target))
                return target;
        }
        return new(-1, -1);
    }

    void BuildFarm()
    {
        var worker = GetIdleWorker();
        if (worker == null) return;

        Vector2Int town = FindMyTownhallPosition();
        Vector2Int pos = FindNearestEmptyCell(town, TerrainTypes.Forest);
        if (pos.x >= 0)
        {
            Debug.Log($"AI {ownerId}: Construyendo granja en {pos}");
            worker.AssignBuildTask(pos, BuildingCodes.Farm);
        }
    }

    void BuildLumbermill()
    {
        var worker = GetIdleWorker();
        if (worker == null) return;

        Vector2Int town = FindMyTownhallPosition();
        Vector2Int pos = FindNearestEmptyCell(town, TerrainTypes.Forest);
        if (pos.x >= 0)
        {
            Debug.Log($"AI {ownerId}: Construyendo aserradero en {pos}");
            worker.AssignBuildTask(pos, BuildingCodes.Lumbermill);
        }
    }

    void BuildHut()
    {
        var worker = GetIdleWorker();
        if (worker == null) return;

        Vector2Int town = FindMyTownhallPosition();
        Vector2Int pos = FindNearestEmptyCell(town, TerrainTypes.Forest);
        if (pos.x >= 0)
        {
            Debug.Log($"AI {ownerId}: Construyendo hut en {pos}");
            worker.AssignBuildTask(pos, BuildingCodes.Hut);
        }
    }

    void BuildBarracks()
    {
        var worker = GetIdleWorker();
        if (worker == null) return;

        Vector2Int town = FindMyTownhallPosition();
        Vector2Int pos = FindNearestEmptyCell(town, TerrainTypes.Forest);
        if (pos.x >= 0)
        {
            Debug.Log($"AI {ownerId}: Construyendo barraca en {pos}");
            worker.AssignBuildTask(pos, BuildingCodes.Barracks);
        }
    }

    void RecruitSoldiers()
    {
        if (CountMyWarriors() >= 10) return;

        Vector2Int town = FindMyTownhallPosition();
        Vector2Int spawn = FindAdjacentFreeCell(town);
        if (spawn.x >= 0)
        {
            Debug.Log($"AI {ownerId}: Reclutando soldado en {spawn}");
            MapLoader.instance.SpawnCharacter(spawn, Character.Type.Warrior, ownerId);
        }
    }

    void BuildAtalaya()
    {
        if (CountBuildings(BuildingCodes.Atalaya) >= 2) return;

        var worker = GetIdleWorker();
        if (worker == null) return;

        Vector2Int town = FindMyTownhallPosition();
        Vector2Int pos = FindNearestEmptyCell(town, TerrainTypes.Forest);
        if (pos.x >= 0)
        {
            Debug.Log($"AI {ownerId}: Construyendo atalaya en {pos}");
            worker.AssignBuildTask(pos, BuildingCodes.Atalaya);
        }
    }

    void BuildAcademy()
    {
        var worker = GetIdleWorker();
        if (worker == null) return;

        Vector2Int town = FindMyTownhallPosition();
        Vector2Int pos = FindNearestEmptyCell(town, TerrainTypes.Forest);
        if (pos.x >= 0)
        {
            Debug.Log($"AI {ownerId}: Construyendo academia en {pos}");
            worker.AssignBuildTask(pos, BuildingCodes.Academy);
        }
    }
}
