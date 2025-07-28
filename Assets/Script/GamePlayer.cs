using System.Collections.Generic;
using UnityEngine;

public class GamePlayer : MonoBehaviour
{
    public static GamePlayer Instance { get; private set; }

    private readonly List<ControlPanelAction> collected = new();

    void Awake()
    {
        Instance = this;
    }

    public void Clear()
    {
        collected.Clear();
    }

    public void AddAction(ControlPanelAction action)
    {
        if (GameTimeManager.Instance != null && !GameTimeManager.Instance.Approve(action))
            return;
        if (ActionManager.Instance != null && !ActionManager.Instance.Approve(action))
            return;
        collected.Add(action);
    }

    public IEnumerable<ControlPanelAction> GetActions()
    {
        return collected;
    }
}
