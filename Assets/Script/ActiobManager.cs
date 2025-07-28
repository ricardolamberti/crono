using System.Collections.Generic;
using UnityEngine;


public class ActionManager : MonoBehaviour
{
    public static ActionManager Instance { get; private set; }

    private readonly Queue<GameAction> queue = new();

    void Awake()
    {
        Instance = this;
    }

    public void Enqueue(GameAction action)
    {
        queue.Enqueue(action);
    }

    void Update()
    {
        while (queue.Count > 0)
        {
            GameAction action = queue.Dequeue();
            if (action.Validate())
            {
                action.Execute();
            }
            else
            {
                Debug.LogWarning($"Rejected action {action.GetType().Name}");
            }
        }
    }
}
