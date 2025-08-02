using System;
using UnityEngine;

public class ControlPanelAction
{
    public string label;
    public Action callback;
    public Sprite icon;

    public ControlPanelAction(string label, Action callback, Sprite icon = null)
    {
        this.label = label;
        this.callback = callback;
        this.icon = icon;
    }
}
