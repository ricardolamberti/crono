using UnityEngine;

public class AIPlayer : Player
{
    private BasicEnemyAI ai;

    public AIPlayer(string id, Vector2Int spawn) : base(id, spawn)
    {
        ai = new BasicEnemyAI(id);
    }

    public override void Initialize()
    {
        base.Initialize();
    }

    public override void Update()
    {
        ai?.Update();
    }
}
