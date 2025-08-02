using System.Collections.Generic;
using static DTO;
using GameConstants;

/// <summary>
/// Definiciones de todas las construcciones y sus evoluciones.
/// Cada nivel se representa con una clase que hereda de su construccion base.
/// </summary>

#region Townhall
public class TownhallLevel1 : Building
{
    public override string Code => BuildingCodes.Townhall;
    public override int Level => 1;
    public override BuildRequirement Cost => new BuildRequirement { wood = 0, gold = 0 };
    public override int VisibilityRadius => 5;
    public override IEnumerable<ControlPanelAction> GetActions(MapCellDTO cell) => BuildingActionHelper.FromLabels(cell, "nuevo obrero" );
}

public class TownhallLevel2 : TownhallLevel1
{
    public override int Level => 2;
    public override BuildRequirement Cost => new BuildRequirement { wood = 50, gold = 50 };
    public override int VisibilityRadius => 5;
    public override IEnumerable<ControlPanelAction> GetActions(MapCellDTO cell) => BuildingActionHelper.FromLabels(cell, "nuevo obrero", "nuevo marino" );
}

public class TownhallLevel3 : TownhallLevel2
{
    public override int Level => 3;
    public override BuildRequirement Cost => new BuildRequirement { wood = 100, gold = 100 };
    public override int VisibilityRadius => 5;
    public override IEnumerable<ControlPanelAction> GetActions(MapCellDTO cell) => BuildingActionHelper.FromLabels(cell, "nuevo obrero", "nuevo marino", "nuevo espia", "nuevo experto" );
}

public class TownhallLevel4 : TownhallLevel3
{
    public override int Level => 4;
    public override BuildRequirement Cost => new BuildRequirement { wood = 150, gold = 150 };
    public override int VisibilityRadius => 5;
    public override IEnumerable<ControlPanelAction> GetActions(MapCellDTO cell) => BuildingActionHelper.FromLabels(cell, "nuevo obrero", "nuevo marino", "nuevo espia", "nuevo experto" );
}
#endregion

#region Barracks
public class BarracksLevel1 : Building
{
    public override string Code => BuildingCodes.Barracks;
    public override int Level => 1;
    public override BuildRequirement Cost => new BuildRequirement { wood = 30, gold = 10 };
    public override IEnumerable<ControlPanelAction> GetActions(MapCellDTO cell) => BuildingActionHelper.FromLabels(cell, "nuevo soldado" );
}

public class BarracksLevel2 : BarracksLevel1
{
    public override int Level => 2;
    public override BuildRequirement Cost => new BuildRequirement { wood = 60, gold = 30 };
    public override IEnumerable<ControlPanelAction> GetActions(MapCellDTO cell) => BuildingActionHelper.FromLabels(cell, "nuevo soldado", "nuevo tanque" );
}

public class BarracksLevel3 : BarracksLevel2
{
    public override int Level => 3;
    public override BuildRequirement Cost => new BuildRequirement { wood = 90, gold = 60 };
    public override IEnumerable<ControlPanelAction> GetActions(MapCellDTO cell) => BuildingActionHelper.FromLabels(cell, "nuevo soldado", "nuevo tanque", "nuevo elite" );
}

public class BarracksLevel4 : BarracksLevel3
{
    public override int Level => 4;
    public override BuildRequirement Cost => new BuildRequirement { wood = 120, gold = 90 };
    public override IEnumerable<ControlPanelAction> GetActions(MapCellDTO cell) => BuildingActionHelper.FromLabels(cell, "nuevo soldado", "nuevo tanque", "nuevo elite", "nuevo destructor" );
}
#endregion

#region Airport
public class AirportLevel1 : Building
{
    public override string Code => BuildingCodes.Airport;
    public override int Level => 1;
    public override BuildRequirement Cost => new BuildRequirement { wood = 40, gold = 20 };
    public override int VisibilityRadius => 6;
    public override IEnumerable<ControlPanelAction> GetActions(MapCellDTO cell) => BuildingActionHelper.FromLabels(cell, "nuevo avion" );
}

public class AirportLevel2 : AirportLevel1
{
    public override int Level => 2;
    public override BuildRequirement Cost => new BuildRequirement { wood = 80, gold = 40 };
    public override int VisibilityRadius => 6;
    public override IEnumerable<ControlPanelAction> GetActions(MapCellDTO cell) => BuildingActionHelper.FromLabels(cell, "nuevo avion", "nuevo bombardero" );
}
#endregion

#region Dock
public class DockLevel1 : Building
{
    public override string Code => BuildingCodes.Dock;
    public override int Level => 1;
    public override BuildRequirement Cost => new BuildRequirement { wood = 25, gold = 10 };
    public override IEnumerable<ControlPanelAction> GetActions(MapCellDTO cell) => BuildingActionHelper.FromLabels(cell, "nuevo pesquero" );
}

public class DockLevel2 : DockLevel1
{
    public override int Level => 2;
    public override BuildRequirement Cost => new BuildRequirement { wood = 50, gold = 30 };
    public override IEnumerable<ControlPanelAction> GetActions(MapCellDTO cell) => BuildingActionHelper.FromLabels(cell, "nuevo pesquero", "nuevo mercante" );
}

public class DockLevel3 : DockLevel2
{
    public override int Level => 3;
    public override BuildRequirement Cost => new BuildRequirement { wood = 75, gold = 60 };
    public override IEnumerable<ControlPanelAction> GetActions(MapCellDTO cell) => BuildingActionHelper.FromLabels(cell, "nuevo pesquero", "nuevo mercante", "nuevo patrulla" );
}

public class DockLevel4 : DockLevel3
{
    public override int Level => 4;
    public override BuildRequirement Cost => new BuildRequirement { wood = 100, gold = 100 };
    public override IEnumerable<ControlPanelAction> GetActions(MapCellDTO cell) => BuildingActionHelper.FromLabels(cell, "nuevo pesquero", "nuevo mercante", "nuevo patrulla", "nuevo portavion" );
}
#endregion

#region Hut
public class HutLevel1 : Building
{
    public override string Code => BuildingCodes.Hut;
    public override int Level => 1;
    public override BuildRequirement Cost => new BuildRequirement { wood = 10 };
    public override IEnumerable<ControlPanelAction> GetActions(MapCellDTO cell) => BuildingActionHelper.FromLabels(cell, "" );
}

public class HutLevel2 : HutLevel1
{
    public override int Level => 2;
    public override BuildRequirement Cost => new BuildRequirement { wood = 20 };
    public override IEnumerable<ControlPanelAction> GetActions(MapCellDTO cell) => BuildingActionHelper.FromLabels(cell,  "" );
}

public class HutLevel3 : HutLevel2
{
    public override int Level => 3;
    public override BuildRequirement Cost => new BuildRequirement { wood = 30 };
    public override IEnumerable<ControlPanelAction> GetActions(MapCellDTO cell) => BuildingActionHelper.FromLabels(cell, "" );
}

public class HutLevel4 : HutLevel3
{
    public override int Level => 4;
    public override BuildRequirement Cost => new BuildRequirement { wood = 40 };
    public override IEnumerable<ControlPanelAction> GetActions(MapCellDTO cell) => BuildingActionHelper.FromLabels(cell, "" );
}
#endregion

#region Farm
public class FarmLevel1 : Building
{
    public override string Code => BuildingCodes.Farm;
    public override int Level => 1;
    public override BuildRequirement Cost => new BuildRequirement { wood = 20 };
    public override IEnumerable<ControlPanelAction> GetActions(MapCellDTO cell) => BuildingActionHelper.FromLabels(cell, "" );
}

public class FarmLevel2 : FarmLevel1
{
    public override int Level => 2;
    public override BuildRequirement Cost => new BuildRequirement { wood = 40 };
    public override IEnumerable<ControlPanelAction> GetActions(MapCellDTO cell) => BuildingActionHelper.FromLabels(cell, "" );
}

public class FarmLevel3 : FarmLevel2
{
    public override int Level => 3;
    public override BuildRequirement Cost => new BuildRequirement { wood = 60 };
    public override IEnumerable<ControlPanelAction> GetActions(MapCellDTO cell) => BuildingActionHelper.FromLabels(cell, "" );
}

public class FarmLevel4 : FarmLevel3
{
    public override int Level => 4;
    public override BuildRequirement Cost => new BuildRequirement { wood = 80 };
    public override IEnumerable<ControlPanelAction> GetActions(MapCellDTO cell) => BuildingActionHelper.FromLabels(cell, "" );
}
#endregion

#region Academy
public class AcademyLevel1 : Building
{
    public override string Code => BuildingCodes.Academy;
    public override int Level => 1;
    public override BuildRequirement Cost => new BuildRequirement { wood = 30, gold = 20 };
    public override IEnumerable<ControlPanelAction> GetActions(MapCellDTO cell) => BuildingActionHelper.FromLabels(cell, "nuevo aprendiz" );
}

public class AcademyLevel2 : AcademyLevel1
{
    public override int Level => 2;
    public override BuildRequirement Cost => new BuildRequirement { wood = 60, gold = 40 };
    public override IEnumerable<ControlPanelAction> GetActions(MapCellDTO cell) => BuildingActionHelper.FromLabels(cell, "nuevo aprendiz", "nuevo medico" );
}

public class AcademyLevel3 : AcademyLevel2
{
    public override int Level => 3;
    public override BuildRequirement Cost => new BuildRequirement { wood = 90, gold = 60 };
    public override IEnumerable<ControlPanelAction> GetActions(MapCellDTO cell) => BuildingActionHelper.FromLabels(cell, "nuevo aprendiz", "nuevo medico", "nuevo erudito" );
}

public class AcademyLevel4 : AcademyLevel3
{
    public override int Level => 4;
    public override BuildRequirement Cost => new BuildRequirement { wood = 120, gold = 80 };
    public override IEnumerable<ControlPanelAction> GetActions(MapCellDTO cell) => BuildingActionHelper.FromLabels(cell, "nuevo aprendiz", "nuevo medico", "nuevo erudito", "nuevo cientifico" );
}
#endregion

#region Atalaya
public class AtalayaLevel1 : Building
{
    public override string Code => BuildingCodes.Atalaya;
    public override int Level => 1;
    public override BuildRequirement Cost => new BuildRequirement { wood = 20 };
    public override int VisibilityRadius => 3 + Level;
    public override int RangedDamage => 4;
    public override WeaponType RangedWeapon => WeaponType.Arrow;
    public override IEnumerable<ControlPanelAction> GetActions(MapCellDTO cell) => BuildingActionHelper.FromLabels(cell, "" );
}

public class AtalayaLevel2 : AtalayaLevel1
{
    public override int Level => 2;
    public override BuildRequirement Cost => new BuildRequirement { wood = 40 };
    public override int VisibilityRadius => 3 + Level;
    public override int RangedDamage => 8;
    public override IEnumerable<ControlPanelAction> GetActions(MapCellDTO cell) => BuildingActionHelper.FromLabels(cell, "" );
}

public class AtalayaLevel3 : AtalayaLevel2
{
    public override int Level => 3;
    public override BuildRequirement Cost => new BuildRequirement { wood = 60 };
    public override int VisibilityRadius => 3 + Level;
    public override int RangedDamage => 16;
    public override IEnumerable<ControlPanelAction> GetActions(MapCellDTO cell) => BuildingActionHelper.FromLabels(cell, "" );
}

public class AtalayaLevel4 : AtalayaLevel3
{
    public override int Level => 4;
    public override BuildRequirement Cost => new BuildRequirement { wood = 80 };
    public override int VisibilityRadius => 3 + Level;
    public override int RangedDamage => 32;
    public override IEnumerable<ControlPanelAction> GetActions(MapCellDTO cell) => BuildingActionHelper.FromLabels(cell, "" );
}
#endregion

#region Wall
public class WallLevel1 : Building
{
    public override string Code => BuildingCodes.Wall;
    public override int Level => 1;
    public override BuildRequirement Cost => new BuildRequirement { wood = 10 };
    public override int MaxResistance => 10;

    public override bool ShouldRotate => false;
    public override string SpriteKey => $"{Code}_{Level}_{(Orientation == Orientation.Horizontal ? "H" : "V")}";

    public override IEnumerable<ControlPanelAction> GetActions(MapCellDTO cell) => BuildingActionHelper.FromLabels(cell, "" );
}

public class WallLevel2 : WallLevel1
{
    public override int Level => 2;
    public override BuildRequirement Cost => new BuildRequirement { wood = 20 };
    public override int MaxResistance => 20;
    public override IEnumerable<ControlPanelAction> GetActions(MapCellDTO cell) => BuildingActionHelper.FromLabels(cell, "" );
}

public class WallLevel3 : WallLevel2
{
    public override int Level => 3;
    public override BuildRequirement Cost => new BuildRequirement { wood = 30 };
    public override int MaxResistance => 30;
    public override IEnumerable<ControlPanelAction> GetActions(MapCellDTO cell) => BuildingActionHelper.FromLabels(cell, "" );
}

public class WallLevel4 : WallLevel3
{
    public override int Level => 4;
    public override BuildRequirement Cost => new BuildRequirement { wood = 40 };
    public override int MaxResistance => 40;
    public override IEnumerable<ControlPanelAction> GetActions(MapCellDTO cell) => BuildingActionHelper.FromLabels(cell, "" );
}
#endregion

#region Sawmill
public class SawmillLevel1 : Building
{
    public override string Code => BuildingCodes.Lumbermill;
    public override int Level => 1;
    public override BuildRequirement Cost => new BuildRequirement { wood = 15, gold = 5 };
    public override IEnumerable<ControlPanelAction> GetActions(MapCellDTO cell) => BuildingActionHelper.FromLabels(cell, "" );
}

public class SawmillLevel2 : SawmillLevel1
{
    public override int Level => 2;
    public override BuildRequirement Cost => new BuildRequirement { wood = 30, gold = 10 };
    public override IEnumerable<ControlPanelAction> GetActions(MapCellDTO cell) => BuildingActionHelper.FromLabels(cell, "" );
}

public class SawmillLevel3 : SawmillLevel2
{
    public override int Level => 3;
    public override BuildRequirement Cost => new BuildRequirement { wood = 45, gold = 20 };
    public override IEnumerable<ControlPanelAction> GetActions(MapCellDTO cell) => BuildingActionHelper.FromLabels(cell, "" );
}
public class SawmillLevel4 : SawmillLevel3
{
    public override int Level => 4;
    public override BuildRequirement Cost => new BuildRequirement { wood = 45, gold = 20 };
    public override IEnumerable<ControlPanelAction> GetActions(MapCellDTO cell) => BuildingActionHelper.FromLabels(cell, "" );
}
#endregion

#region CronoExtractor
public class CronoExtractorLevel1 : Building
{
    public override string Code => BuildingCodes.Extractor;
    public override int Level => 1;
    public override BuildRequirement Cost => new BuildRequirement { gold = 20, wood = 20, sciencePoints = 10 };
    public override IEnumerable<ControlPanelAction> GetActions(MapCellDTO cell) => BuildingActionHelper.FromLabels(cell, "Nuevo hechicero", "Asistencia futuro" );
}

public class CronoExtractorLevel2 : CronoExtractorLevel1
{
    public override int Level => 2;
    public override BuildRequirement Cost => new BuildRequirement { gold = 40, wood = 40, sciencePoints = 20 };
    public override IEnumerable<ControlPanelAction> GetActions(MapCellDTO cell) => BuildingActionHelper.FromLabels(cell, "Nuevo hechicero", "Asistencia futuro", "Misiones pasado" );
}

public class CronoExtractorLevel3 : CronoExtractorLevel2
{
    public override int Level => 3;
    public override BuildRequirement Cost => new BuildRequirement { gold = 60, wood = 60, sciencePoints = 30 };
    public override IEnumerable<ControlPanelAction> GetActions(MapCellDTO cell) => BuildingActionHelper.FromLabels(cell, "Nuevo hechicero", "Asistencia futuro", "Misiones pasado", "proteccion" );
}

public class CronoExtractorLevel4 : CronoExtractorLevel3
{
    public override int Level => 4;
    public override BuildRequirement Cost => new BuildRequirement { gold = 80, wood = 80, sciencePoints = 40 };
    public override IEnumerable<ControlPanelAction> GetActions(MapCellDTO cell) => BuildingActionHelper.FromLabels(cell, "Nuevo hechicero", "Asistencia futuro", "Misiones pasado", "proteccion", "envio tabla" );
}
#endregion

#region Mine
public class MineLevel1 : Building
{
    public override string Code => BuildingCodes.Mine;
    public override int Level => 1;
    public override BuildRequirement Cost => new BuildRequirement { wood = 15, gold = 5 };
    public override IEnumerable<ControlPanelAction> GetActions(MapCellDTO cell) => BuildingActionHelper.FromLabels(cell, "");
}

public class MineLevel2 : MineLevel1
{
    public override int Level => 2;
    public override BuildRequirement Cost => new BuildRequirement { wood = 30, gold = 10 };
    public override IEnumerable<ControlPanelAction> GetActions(MapCellDTO cell) => BuildingActionHelper.FromLabels(cell, "");
}

public class MineLevel3 : MineLevel2
{
    public override int Level => 3;
    public override BuildRequirement Cost => new BuildRequirement { wood = 45, gold = 20 };
    public override IEnumerable<ControlPanelAction> GetActions(MapCellDTO cell) => BuildingActionHelper.FromLabels(cell, "");
}

public class MineLevel4 : MineLevel3
{
    public override int Level => 4;
    public override BuildRequirement Cost => new BuildRequirement { wood = 45, gold = 20 };
    public override IEnumerable<ControlPanelAction> GetActions(MapCellDTO cell) => BuildingActionHelper.FromLabels(cell, "");
}
#endregion

#region Bridge
public class BridgeLevel1 : Building
{
    public override string Code => BuildingCodes.Bridge;
    public override int Level => 1;
    public override BuildRequirement Cost => new BuildRequirement { wood = 10 };
    public override bool ShouldRotate => false;
    public override string SpriteKey => $"{Code}_{(Orientation == Orientation.Horizontal ? "H" : "V")}";
    public override IEnumerable<ControlPanelAction> GetActions(MapCellDTO cell) => BuildingActionHelper.FromLabels(cell);
}

public class BridgeLevel2 : BridgeLevel1
{
    public override int Level => 2;
    public override BuildRequirement Cost => new BuildRequirement { wood = 20 };
}

public class BridgeLevel3 : BridgeLevel2
{
    public override int Level => 3;
    public override BuildRequirement Cost => new BuildRequirement { wood = 30 };
}
#endregion

#region TemporalBreach
public class TemporalBreach : Building
{
    public override string Code => BuildingCodes.TemporalBreach;
    public override int Level => 1;
    public override BuildRequirement Cost => new BuildRequirement();
    public override IEnumerable<ControlPanelAction> GetActions(MapCellDTO cell)
        => BuildingActionHelper.FromLabels(cell,
            "pedir recursos", "grabar estado", "recuperar");
}
#endregion