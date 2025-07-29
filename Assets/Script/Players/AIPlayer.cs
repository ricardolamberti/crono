using UnityEngine;

public class AIPlayer : Player
{
    private BasicEnemyAI ai;

    public AIPlayer(string id, Vector2Int spawn) : base(id, spawn)
    {
        ai = new BasicEnemyAI(id, spawn);
    }

    public override void Initialize()
    {
        if (MapLoader.instance != null)
        {
            MapLoader.instance.SpawnCharacter(SpawnPosition, Character.Type.Worker, Id);
        }
    }

    public override void Update()
    {
        ai?.Update();
    }
}
