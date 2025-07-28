using UnityEngine;

public class WarriorRole : CharacterRole
{
    public override string Code => "warrior";

    public override void ProposeActions(Character character, GamePlayer player)
    {
        player.AddAction(new ControlPanelAction("Atacar", () =>
        {
            GameEvents.RequestAttack(character);
        }));
    }
}
