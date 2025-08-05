using System;
using System.Linq;
using UnityEngine;
using static DTO;
using GameConstants;

[RequireComponent(typeof(TileClickHandler))]
public class TileActionProvider : MonoBehaviour, IActionProposer
{
    TileClickHandler handler;
    static Sprite demolishIcon;

    static readonly System.Collections.Generic.Dictionary<string, string[]> terrainBuildings = new()
    {
        { TerrainTypes.Mountain, new[] { BuildingCodes.Mine } },
        { TerrainTypes.Water, new[] { BuildingCodes.Dock, BuildingCodes.Bridge } },
        { TerrainTypes.Forest, new[] { BuildingCodes.Hut, BuildingCodes.Lumbermill, BuildingCodes.Farm, BuildingCodes.Academy, BuildingCodes.Barracks, BuildingCodes.Wall, BuildingCodes.Atalaya, BuildingCodes.Airport } }
    };

    void Awake()
    {
        handler = GetComponent<TileClickHandler>();
        if (demolishIcon == null)
            demolishIcon = Resources.Load<Sprite>("Icons/bomb");
    }

    public void ProposeActions(GamePlayer player)
    {
        var cell = handler.GetCellData();
        if (cell == null) return;

        if (!string.IsNullOrEmpty(cell.building))
        {
            if (!string.IsNullOrEmpty(cell.owner) && cell.owner != "player1")
            {
                player.AddAction(new ControlPanelAction("Atacar", () => Debug.Log("Atacando...")));
                return;
            }

            Vector2Int posCell = new(cell.x, cell.y);
            if (MapState.buildings.TryGetValue(posCell, out var building))
            {
                foreach (var action in building.GetActions(cell))
                    player.AddAction(action);
            }

            if (BuildingEvolutionMatrix.TryGetEvolution(cell.building, cell.level, out var evo))
            {
                if (ControlPanel.Instance.freeResource || GameState.playerResources.science >= evo.requiredScience)
                {
                    var upgradeBuilding = BuildingFactory.Create(evo.next, evo.level);
                    var icon = upgradeBuilding != null ? MapLoader.instance.GetBuildingSprite(upgradeBuilding.SpriteKey) : null;
                    player.AddAction(new ControlPanelAction($"Mejorar a {evo.next}", () =>
                    {
                        var req = BuildRules.GetRequirements(evo.next);
                        if (!ControlPanel.Instance.freeResource && !GameState.playerResources.HasEnough(req))
                        {
                            Debug.Log($"No hay recursos suficientes para evolucionar {evo.next}");
                            return;
                        }
                        GameState.playerResources.Consume(req);
                        GameState.playerResources.science -= evo.requiredScience;
                        var newBuilding = BuildingFactory.Create(evo.next, evo.level);
                        MapLoader.instance.UpgradeBuilding(new Vector2Int(cell.x, cell.y), newBuilding);
                        GameEvents.RaiseSelection(gameObject);
                    }, icon));
                }
            }

            if (cell.building != BuildingCodes.Townhall && cell.building != BuildingCodes.TemporalBreach)
            {
                player.AddAction(new ControlPanelAction("Derrumbar", () => {
                    MapLoader.instance.DemolishBuilding(new Vector2Int(cell.x, cell.y));
                    GameEvents.RaiseSelection(gameObject);
                }, demolishIcon));
            }
            return;
        }

        Vector2Int pos = new(cell.x, cell.y);

        void TryBuild(string text, string code)
        {
            var building = BuildingFactory.Create(code);
            var req = building != null ? building.Cost : new BuildRequirement();
            var icon = building != null ? MapLoader.instance.GetBuildingSprite(building.SpriteKey) : null;
            player.AddAction(new ControlPanelAction(text, () => {
                if (!ControlPanel.Instance.freeResource)
                {
                    if (!GameState.playerResources.HasEnough(req))
                    {
                        Debug.Log($"No hay recursos suficientes para construir {code}");
                        return;
                    }
                }

                var worker = FindFreeWorker(pos);
                if (worker != null)
                {
                    worker.SetGatherRoute(null);
                    GameState.playerResources.Consume(req);
                    ActionManager.Instance.Enqueue(new BuildAction(worker, pos, code));
                }
            }, icon));
        }

        if (terrainBuildings.TryGetValue(cell.terrain, out var options))
        {
            if (cell.terrain == TerrainTypes.Forest && !TownhallExists())
            {
                var townhall = BuildingFactory.Create(BuildingCodes.Townhall);
                var iconTown = townhall != null ? MapLoader.instance.GetBuildingSprite(townhall.SpriteKey) : null;
                player.AddAction(new ControlPanelAction("Construir casa central", () => {
                    var worker = FindFreeWorker(new Vector2Int(cell.x, cell.y));
                    if (worker != null)
                        ActionManager.Instance.Enqueue(new BuildAction(worker, new Vector2Int(cell.x, cell.y), BuildingCodes.Townhall));
                    else
                        Debug.Log("No hay obreros disponibles.");
                }, iconTown));
            }
            else
            {
                foreach (var bCode in options)
                    TryBuild($"Construir {bCode}", bCode);
            }
        }

        if (cell.resources?.crono > 0)
        {
            TryBuild("Construir extractor de crono", BuildingCodes.Extractor);
        }
    }

    Character FindFreeWorker(Vector2Int targetPos)
    {
        var all = GameObject.FindObjectsOfType<Character>();

        Character best = null;
        float bestDist = float.MaxValue;

        // Primero buscar un obrero que no esté realizando ninguna tarea
        foreach (var c in all)
        {
            if (c.owner != "player1") continue;
            if (c.role is WorkerRole && c.currentTask == Character.Task.None)
            {
                float dist = Vector2Int.Distance(c.GetGridPosition(), targetPos);
                if (dist < bestDist)
                {
                    bestDist = dist;
                    best = c;
                }
            }
        }

        if (best != null)
            return best;

        // Si no hay obreros libres, tomar cualquiera en modo automático
        bestDist = float.MaxValue;
        foreach (var c in all)
        {
            if (c.owner != "player1") continue;
            if (c.role is WorkerRole && c.controlMode == Character.ControlMode.Automatic)
            {
                float dist = Vector2Int.Distance(c.GetGridPosition(), targetPos);
                if (dist < bestDist)
                {
                    bestDist = dist;
                    best = c;
                }
            }
        }

        return best;
    }


    bool TownhallExists()
    {
        return MapState.cellMap.Values.Any(c =>
            c.building == BuildingCodes.Townhall && c.owner == "player1");
    }
}
