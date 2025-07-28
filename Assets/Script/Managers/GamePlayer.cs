using System.Collections.Generic;
using UnityEngine;

public class GamePlayer : MonoBehaviour
{
    public static GamePlayer Instance { get; private set; }

    private readonly List<ControlPanelAction> collected = new();
    private readonly List<InfoItem> infoCollected = new();

    void Awake()
    {
        Instance = this;
    }

    public void Clear()
    {
        collected.Clear();
        infoCollected.Clear();
    }

    public void AddAction(ControlPanelAction action)
    {
        if (GameTimeManager.Instance != null && !GameTimeManager.Instance.Approve(action))
            return;
        if (ActionManager.Instance != null && !ActionManager.Instance.Approve(action))
            return;
        collected.Add(action);
    }

    public void AddInfo(InfoItem info)
    {
        if (GameTimeManager.Instance != null && !GameTimeManager.Instance.Approve(info))
            return;
        if (ActionManager.Instance != null && !ActionManager.Instance.Approve(info))
            return;
        infoCollected.Add(info);
    }

    public IEnumerable<ControlPanelAction> GetActions()
    {
        return collected;
    }

    public IEnumerable<InfoItem> GetInfo()
    {
        return infoCollected;
    }
}
