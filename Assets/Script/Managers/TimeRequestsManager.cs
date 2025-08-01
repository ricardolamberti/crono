using System.Collections.Generic;
using UnityEngine;

public class TimeRequest
{
    public TimeRequestConfig config;
    public int deliverYear;
    public TimeBreach breach;
    public int futureYears;
}

public class TimeRequestsManager : MonoBehaviour
{
    public static TimeRequestsManager Instance { get; private set; }

    private readonly List<TimeRequest> requests = new();

    void Awake()
    {
        Instance = this;
    }

    void OnEnable()
    {
        GameTimeManager.OnDateChanged += OnDateChanged;
    }

    void OnDisable()
    {
        GameTimeManager.OnDateChanged -= OnDateChanged;
    }

    void OnDateChanged(int month, int year)
    {
        ProcessTurn();
    }

    public void RegisterRequest(TimeBreach breach, TimeRequestConfig config, int futureYears)
    {
        var req = new TimeRequest
        {
            breach = breach,
            config = config,
            futureYears = futureYears,
            deliverYear = GameTimeManager.CurrentYear + futureYears
        };
        requests.Add(req);
    }

    void ProcessTurn()
    {
        for (int i = requests.Count - 1; i >= 0; i--)
        {
            var r = requests[i];
            if (GameTimeManager.CurrentYear >= r.deliverYear)
            {
                Deliver(r);
                requests.RemoveAt(i);
            }
        }
    }

    void Deliver(TimeRequest req)
    {
        switch (req.config.id)
        {
            case "gold":
                GameState.playerResources.gold += 10;
                break;
            case "wood":
                GameState.playerResources.wood += 10;
                break;
            case "food":
                GameState.playerResources.food += 10;
                break;
            case "worker":
                MapLoader.instance.SpawnCharacter(req.breach.Position, Character.Type.Worker, "player1");
                break;
            default:
                MapLoader.instance.SpawnCharacter(req.breach.Position, Character.Type.Warrior, "player1");
                break;
        }
    }
}
