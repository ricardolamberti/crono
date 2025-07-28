using UnityEngine;
using System.Collections.Generic;

public class BasicEnemyAI
{
    private readonly string ownerId;
    private float timer = 0f;
    private readonly float decisionInterval = 5f;

    public BasicEnemyAI(string owner)
    {
        ownerId = owner;
    }

    public void Update()
    {
        timer += Time.deltaTime;
        if (timer >= decisionInterval)
        {
            timer = 0f;
            EvaluateCharacters();
        }
    }

    void EvaluateCharacters()
    {
        Character[] characters = GameObject.FindObjectsOfType<Character>();
        foreach (var c in characters)
        {
            if (c.owner != ownerId) continue;

            if (c.currentTask == Character.Task.None)
            {
                c.gatherTask = Character.GatherTask.Food;
                c.PlanGatherRoute();
            }
        }
    }
}
