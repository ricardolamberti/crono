using UnityEngine;

public class WarriorRole : CharacterRole
{
    public override string Code => "warrior";

    public WarriorClass warriorClass = WarriorClass.SoldadoRaso;

    public WarriorStats Stats => WarriorStatsMatrix.stats[warriorClass];

    public override void ProposeActions(Character character, GamePlayer player)
    {
        player.AddAction(new ControlPanelAction("Atacar", () =>
        {
            GameEvents.RequestAttack(character);
        }));
    }
}
