using System;
using System.Collections.Generic;
using UnityEngine;
using static DTO;

public static class BuildingActionHelper
{
    public static IEnumerable<ControlPanelAction> FromLabels(MapCellDTO cell, params string[] labels)
    {
        foreach (var label in labels)
        {
            var lower = label.ToLowerInvariant();
            if (lower.Contains("obrero"))
                yield return SpawnCharacter(label, cell, Character.Type.Worker);
            else if (lower.Contains("cientifico"))
                yield return SpawnCharacter(label, cell, Character.Type.Scientist);
            else if (lower.Contains("soldado"))
                yield return SpawnCharacter(label, cell, Character.Type.Warrior);
            else if (lower.Contains("grabar estado"))
                yield return SaveState(label);
            else if (lower.Contains("recuperar"))
                yield return ShowLoadMenu(label);
            else if (lower.Contains("pedir recursos"))
                yield return ShowRequestResource(label);
            else
                yield return NotImplemented(label);
        }
    }

    public static ControlPanelAction SpawnCharacter(string label, MapCellDTO cell, Character.Type type)
    {
        return new ControlPanelAction(label, () => SpawnCharacterNear(cell, type));
    }

    public static ControlPanelAction NotImplemented(string label)
    {
        return new ControlPanelAction(label, () => Debug.Log($"Acción '{label}' no implementada."));
    }

    public static ControlPanelAction SaveState(string label)
    {
        return new ControlPanelAction(label, () => SaveSystem.SaveGame());
    }

    public static ControlPanelAction ShowLoadMenu(string label)
    {
        return new ControlPanelAction(label, () => ControlPanel.Instance.ShowLoadMenu());
    }

    public static ControlPanelAction ShowRequestResource(string label)
    {
        return new ControlPanelAction(label, () => ControlPanel.Instance.ShowRequestResource());

    }
  

public static void SpawnCharacterNear(MapCellDTO cell, Character.Type type)
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
}
