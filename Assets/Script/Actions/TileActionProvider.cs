using System;
using System.Linq;
using UnityEngine;
using static DTO;

[RequireComponent(typeof(TileClickHandler))]
public class TileActionProvider : MonoBehaviour, IActionProposer
{
    TileClickHandler handler;

    static readonly System.Collections.Generic.Dictionary<string, string[]> terrainBuildings = new()
    {
        { "mountain", new[] { "mine" } },
        { "water", new[] { "dock" } },
        { "forest", new[] { "hut", "lumbermill", "farm", "academy", "barracks" } }
    };

    void Awake()
    {
        handler = GetComponent<TileClickHandler>();
    }

    public void ProposeActions(GamePlayer player)
    {
        var cell = handler.GetCellData();
        if (cell == null) return;

        if (!string.IsNullOrEmpty(cell.building))
        {
            Vector2Int posCell = new(cell.x, cell.y);
            if (MapState.buildings.TryGetValue(posCell, out var building))
            {
                foreach (var act in building.Actions)
                {
                    string lower = act.ToLowerInvariant();
                    if (lower.Contains("obrero"))
                    {
                        player.AddAction(new ControlPanelAction(act, () =>
                        {
                            SpawnWorkerNear(cell, Character.Type.Worker);
                        }));
                    }
                    else if (lower.Contains("cientifico"))
                    {
                        player.AddAction(new ControlPanelAction(act, () =>
                        {
                            SpawnWorkerNear(cell, Character.Type.Scientist);
                        }));
                    }
                    else if (lower.Contains("soldado"))
                    {
                        player.AddAction(new ControlPanelAction(act, () =>
                        {
                            SpawnWorkerNear(cell, Character.Type.Warrior);
                        }));
                    }
                    else
                    {
                        player.AddAction(new ControlPanelAction(act, () =>
                        {
                            Debug.Log($"Acción '{act}' no implementada.");
                        }));
                    }
                }
            }

            if (BuildingEvolutionMatrix.TryGetEvolution(cell.building, cell.level, out var evo))
            {
                if (ControlPanel.Instance.freeResource || GameState.playerResources.science >= evo.requiredScience)
                {
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
                        MapLoader.instance.UpgradeBuilding(new Vector2Int(cell.x, cell.y), evo.next, evo.level);
                        GameEvents.RaiseSelection(gameObject);
                    }));
                }
            }

            if (cell.building != "townhall")
            {
                player.AddAction(new ControlPanelAction("Derrumbar", () => {
                    MapLoader.instance.DemolishBuilding(new Vector2Int(cell.x, cell.y));
                    GameEvents.RaiseSelection(gameObject);
                }));
            }
            return;
        }

        Vector2Int pos = new(cell.x, cell.y);

        void TryBuild(string text, string code)
        {
            player.AddAction(new ControlPanelAction(text, () => {
                var building = BuildingFactory.Create(code);
                var req = building != null ? building.Cost : new BuildRequirement();
                if (!ControlPanel.Instance.freeResource)
                {
                    if (!GameState.playerResources.HasEnough(req))
                    {
                        Debug.Log($"No hay recursos suficientes para construir {code}");
                        return;
                    }
                }

                var worker = FindFreeWorker();
                if (worker != null)
                {
                    worker.SetGatherRoute(null);
                    GameState.playerResources.Consume(req);
                    ActionManager.Instance.Enqueue(new BuildAction(worker, pos, code));
                }
            }));
        }

        if (terrainBuildings.TryGetValue(cell.terrain, out var options))
        {
            if (cell.terrain == "forest" && !TownhallExists())
            {
                player.AddAction(new ControlPanelAction("Construir casa central", () => {
                    var worker = FindFreeWorker();
                    if (worker != null)
                        ActionManager.Instance.Enqueue(new BuildAction(worker, new Vector2Int(cell.x, cell.y), "townhall"));
                    else
                        Debug.Log("No hay obreros disponibles.");
                }));
            }
            else
            {
                foreach (var bCode in options)
                    TryBuild($"Construir {bCode}", bCode);
            }
        }

        if (cell.resources?.crono > 0)
        {
            TryBuild("Construir extractor de crono", "extractor");
        }
    }

    Character FindFreeWorker()
    {
        var all = GameObject.FindObjectsOfType<Character>();
        foreach (var c in all)
        {
            if (c.role is WorkerRole &&
                c.controlMode == Character.ControlMode.Automatic &&
                c.currentTask == Character.Task.None)
            {
                return c;
            }
        }
        foreach (var c in all)
        {
            if (c.role is WorkerRole &&
                c.controlMode == Character.ControlMode.Manual &&
                c.currentTask == Character.Task.None)
            {
                return c;
            }
        }
        return null;
    }

    void SpawnWorkerNear(MapCellDTO cell, Character.Type type)
    {
        var req = BuildRules.TakeRequirements(type);
        if (!ControlPanel.Instance.freeResource)
        {
            if (!GameState.playerResources.HasEnough(req))
            {
                Debug.Log($"No hay recursos suficientes para construir {type}");
                return;
            }
        }

        Vector2Int basePos = new(cell.x, cell.y);
        foreach (var dir in new[] { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right })
        {
            Vector2Int target = basePos + dir;
            if (MapState.cellMap.TryGetValue(target, out var targetCell)
                && string.IsNullOrEmpty(targetCell.building)
                && MapLoader.instance.IsPositionFree(target))
            {
                ActionManager.Instance.Enqueue(new SpawnCharacterAction(target, type, "player1"));
                return;
            }
        }

        Debug.Log("No hay espacio disponible junto al townhall.");
    }

    bool TownhallExists()
    {
        return MapState.cellMap.Values.Any(c =>
            c.building == "townhall" && c.owner == "player1");
    }
}
