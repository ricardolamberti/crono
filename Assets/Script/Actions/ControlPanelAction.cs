using System;

public class ControlPanelAction
{
    public string label;
    public Action callback;
    public ControlPanelAction(string label, Action callback)
    {
        this.label = label;
        this.callback = callback;
    }
}
