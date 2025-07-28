using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public abstract class GameAction
{
    public abstract bool Validate();
    public abstract void Execute();
}

