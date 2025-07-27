using UnityEngine;

[System.Serializable]
public class SpawnCharacterAction : GameAction
{
    public Character.Type characterType;
    public Vector2Int position;
    public string owner;

    public SpawnCharacterAction(Vector2Int pos, Character.Type type, string ownerId)
    {
        position = pos;
        characterType = type;
        owner = ownerId;
    }

    public override bool Validate()
    {
        return MapLoader.instance != null && MapLoader.instance.IsPositionFree(position);
    }

    public override void Execute()
    {
        MapLoader.instance.SpawnCharacter(position, characterType, owner);
    }
}
