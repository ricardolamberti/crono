using UnityEngine;
using GameConstants;

public abstract class Player
{
    public string Id { get; private set; }
    public Vector2Int SpawnPosition { get; private set; }

    protected Player(string id, Vector2Int spawn)
    {
        Id = id;
        SpawnPosition = spawn;
    }

    public virtual void Initialize()
    {
        if (MapLoader.instance != null)
        {
            MapLoader.instance.SpawnCharacter(SpawnPosition, Character.Type.Worker, Id);
            MapLoader.instance.RevealRadius(SpawnPosition, 1);
            MapLoader.instance.PlaceBuilding(SpawnPosition, GameConstants.BuildingCodes.TemporalBreach, Id);
        }
    }

    public virtual void Update() { }
}
