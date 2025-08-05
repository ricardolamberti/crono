using System.Collections.Generic;

public enum WeaponType { None, Arrow, Bullet, Ray }
public enum WarriorClass { SoldadoRaso, Arquero, Tanque, Cybor, Dron }

public class WarriorStats
{
    public int shortRangeDamage;
    public int longRangeDamage;
    public WeaponType weapon;

    public WarriorStats(int shortDmg, int longDmg, WeaponType weap)
    {
        shortRangeDamage = shortDmg;
        longRangeDamage = longDmg;
        weapon = weap;
    }
}

public static class WarriorStatsMatrix
{
    public static readonly Dictionary<WarriorClass, WarriorStats> stats = new()
    {
        { WarriorClass.SoldadoRaso, new WarriorStats(2, 0, WeaponType.None) },
        { WarriorClass.Arquero,     new WarriorStats(2, 2, WeaponType.Arrow) },
        { WarriorClass.Tanque,      new WarriorStats(4, 3, WeaponType.Bullet) },
        { WarriorClass.Cybor,       new WarriorStats(5, 4, WeaponType.Bullet) },
        { WarriorClass.Dron,        new WarriorStats(6, 6, WeaponType.Ray) },
    };
}
