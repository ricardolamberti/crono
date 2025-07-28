using UnityEngine;

public abstract class CharacterRole : MonoBehaviour
{
    public abstract string Code { get; }

    public virtual void ProposeActions(Character character, GamePlayer player)
    {
    }
}
