using UnityEngine;

public class ScientistRole : CharacterRole
{
    public override string Code => "scientist";

    public override void ProposeActions(Character character, GamePlayer player)
    {
        player.AddAction(new ControlPanelAction("Curar", () =>
        {
            GameEvents.RequestHeal(character);
        }));
    }
}
