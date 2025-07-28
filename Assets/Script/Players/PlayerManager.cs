using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    public static PlayerManager Instance { get; private set; }

    public int playerCount = 2;

    private readonly List<Player> players = new();
    public Vector2Int Player1Spawn { get; private set; }

    readonly Vector2Int[] defaultSpawns = new Vector2Int[]
    {
        new Vector2Int(1,1),
        new Vector2Int(18,18),
        new Vector2Int(1,18),
        new Vector2Int(18,1)
    };

    void Awake()
    {
        Instance = this;
    }

    public void InitializePlayers()
    {
        players.Clear();
        if (playerCount < 1) playerCount = 1;

        // buscar posiciones de inicio definidas en el mapa
        Dictionary<string, Vector2Int> mapSpawns = new();
        foreach (var kv in MapState.cellMap)
        {
            if (!string.IsNullOrEmpty(kv.Value.start_player))
            {
                mapSpawns[kv.Value.start_player] = kv.Key;
            }
        }

        Vector2Int spawn = mapSpawns.ContainsKey("player1")
            ? mapSpawns["player1"]
            : (defaultSpawns.Length > 0 ? defaultSpawns[0] : new Vector2Int(1,1));

        Player1Spawn = spawn;
        players.Add(new HumanPlayer("player1", spawn));

        for (int i = 1; i < playerCount; i++)
        {
            string id = $"ai{i}";
            Vector2Int pos = mapSpawns.ContainsKey(id)
                ? mapSpawns[id]
                : (i < defaultSpawns.Length ? defaultSpawns[i] : new Vector2Int(1 + i*2, 1 + i*2));
            players.Add(new AIPlayer(id, pos));
        }

        foreach (var p in players)
            p.Initialize();

        Vector3 focus = GridUtils.GridToWorld(Player1Spawn);
        Camera.main.transform.position = focus + new Vector3(5, 10, -5);
        Camera.main.transform.LookAt(focus);
    }

    void Update()
    {
        foreach (var p in players)
            p.Update();
    }
}
