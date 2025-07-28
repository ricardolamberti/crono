using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    public static PlayerManager Instance { get; private set; }

    public int playerCount = 2;

    private readonly List<Player> players = new();

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
        Vector2Int spawn = defaultSpawns.Length > 0 ? defaultSpawns[0] : new Vector2Int(1,1);
        players.Add(new HumanPlayer("player1", spawn));

        for (int i = 1; i < playerCount; i++)
        {
            Vector2Int pos = i < defaultSpawns.Length ? defaultSpawns[i] : new Vector2Int(1 + i*2, 1 + i*2);
            players.Add(new AIPlayer($"ai{i}", pos));
        }

        foreach (var p in players)
            p.Initialize();
    }

    void Update()
    {
        foreach (var p in players)
            p.Update();
    }
}
