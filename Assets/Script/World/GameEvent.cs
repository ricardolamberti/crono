using System;
using UnityEngine;

public static class GameEvents
{
    public static Action<GameObject> OnSelectionChanged;
    public static event Action<Character> OnAttackRequested;
    public static event Action<Character> OnHealRequested;
    public static event Action<Character> OnMovementRequested;

    public static void RequestAttack(Character character) => OnAttackRequested?.Invoke(character);
    public static void RequestHeal(Character character) => OnHealRequested?.Invoke(character);

    public static void RequestMovement(Character character) => OnMovementRequested?.Invoke(character);


    public static void RaiseSelection(GameObject selected)
    {
        OnSelectionChanged?.Invoke(selected);
    }


}