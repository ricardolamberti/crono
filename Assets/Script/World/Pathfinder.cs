using System.Collections.Generic;
using UnityEngine;
using static DTO;
using GameConstants;

public static class Pathfinder
{
    public class Node
    {
        public Vector2Int pos;
        public Node parent;
        public int gCost, hCost;
        public int FCost => gCost + hCost;
    }

    public static List<Vector2Int> FindPath(
        Vector2Int start,
        Vector2Int target,
        Dictionary<Vector2Int, MapCellDTO> cellMap,
        string actorId)
    {
        var open = new List<Node>();
        var closed = new HashSet<Vector2Int>();

        Node startNode = new() { pos = start, gCost = 0, hCost = Heuristic(start, target) };
        open.Add(startNode);

        Node bestAlternative = null;
        int bestHeuristic = int.MaxValue;

        while (open.Count > 0)
        {
            // ✅ buscar el mejor nodo (sin ordenar toda la lista)
            Node current = open[0];
            int currentIndex = 0;
            for (int i = 1; i < open.Count; i++)
            {
                if (open[i].FCost < current.FCost || (open[i].FCost == current.FCost && open[i].hCost < current.hCost))
                {
                    current = open[i];
                    currentIndex = i;
                }
            }
            open.RemoveAt(currentIndex);
            closed.Add(current.pos);

            if (current.pos == target)
                return BuildPath(current);

            if (current.hCost < bestHeuristic)
            {
                bestHeuristic = current.hCost;
                bestAlternative = current;
            }

            foreach (var dir in Directions)
            {
                Vector2Int nextPos = current.pos + dir;
                if (closed.Contains(nextPos)) continue;
                if (!cellMap.TryGetValue(nextPos, out var cell)) continue;
                if (!IsWalkable(cell.terrain, cell.building, cell.owner, actorId)) continue;

                int moveCost = (dir.x == 0 || dir.y == 0) ? 10 : 14; // 10: ortogonal, 14: diagonal (aprox √2 * 10)
                int gCost = current.gCost + moveCost;
                int hCost = Heuristic(nextPos, target);

                Node next = new()
                {
                    pos = nextPos,
                    parent = current,
                    gCost = gCost,
                    hCost = hCost
                };

                var existing = open.Find(n => n.pos == nextPos);
                if (existing == null)
                {
                    open.Add(next);
                }
                else if (next.FCost < existing.FCost || (next.FCost == existing.FCost && next.gCost < existing.gCost))
                {
                    open.Remove(existing);
                    open.Add(next);
                }
            }
        }

        // ❗ Si no se llega al objetivo, retornar el camino al más cercano
        if (bestAlternative != null)
            return BuildPath(bestAlternative);

        return new List<Vector2Int> { start }; // quedarse quieto
    }

    private static List<Vector2Int> BuildPath(Node end)
    {
        var path = new List<Vector2Int>();
        for (var n = end; n != null; n = n.parent)
            path.Insert(0, n.pos);
        return path;
    }

    // ✅ Heurística Chebyshev (diagonal distance)
    private static int Heuristic(Vector2Int a, Vector2Int b)
    {
        int dx = Mathf.Abs(a.x - b.x);
        int dy = Mathf.Abs(a.y - b.y); 
        return 10 * (dx + dy) + (14 - 2 * 10) * Mathf.Min(dx, dy);
    }

    public static bool IsWalkable(string terrain, string building = null, string cellOwner = null, string actorId = null)
    {
        if (building == BuildingCodes.Bridge)
            return true;
        if (building == BuildingCodes.Wall && !string.IsNullOrEmpty(cellOwner) && cellOwner != actorId)
            return false;
        return terrain != TerrainTypes.Water && terrain != TerrainTypes.Mountain;
    }

    // ✅ 8 direcciones (N, S, E, O, NE, NW, SE, SW)
    private static readonly List<Vector2Int> Directions = new()
    {
        Vector2Int.up,
        Vector2Int.down,
        Vector2Int.left,
        Vector2Int.right,
        new Vector2Int(1, 1),   // NE
        new Vector2Int(-1, 1),  // NW
        new Vector2Int(1, -1),  // SE
        new Vector2Int(-1, -1)  // SW
    };
}
