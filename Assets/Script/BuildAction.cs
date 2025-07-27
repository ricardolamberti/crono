using UnityEngine;

[System.Serializable]
public class BuildAction : GameAction
{
    public string buildingCode;
    public Vector2Int position;
    public Character builder;

    public BuildAction(Character builder, Vector2Int pos, string code)
    {
        this.builder = builder;
        position = pos;
        buildingCode = code;
    }

    public override bool Validate()
    {
        if (builder == null) return false;
        if (!MapState.cellMap.TryGetValue(position, out var cell)) return false;
        if (!string.IsNullOrEmpty(cell.building)) return false;
        return true;
    }

    public override void Execute()
    {
        builder.AssignBuildTask(position, buildingCode);
    }
}
