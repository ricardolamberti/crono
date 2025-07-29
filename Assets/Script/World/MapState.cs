using static DTO;
using System.Collections.Generic;
using UnityEngine;

public static class MapState
{
    public static Dictionary<Vector2Int, MapCellDTO> cellMap = new();
    public static Dictionary<Vector2Int, Building> buildings = new();
}
