using UnityEngine;

public class AIManager : MonoBehaviour
{
    public string enemyId = "enemy";
    private BasicEnemyAI ai;
    public Vector2Int initialSpawn = new Vector2Int(3, 3);

    void Start()
    {
        ai = new BasicEnemyAI(enemyId);
        if (MapLoader.instance != null)
            MapLoader.instance.SpawnCharacter(initialSpawn, Character.Type.Worker, enemyId);
    }

    void Update()
    {
        ai?.Update();
    }
}
