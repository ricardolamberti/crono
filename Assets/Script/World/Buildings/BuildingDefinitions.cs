using System.Collections.Generic;

/// <summary>
/// Definiciones de todas las construcciones y sus evoluciones.
/// Cada nivel se representa con una clase que hereda de su construccion base.
/// </summary>

#region Townhall
public class TownhallLevel1 : Building
{
    public override string Code => "townhall";
    public override int Level => 1;
    public override BuildRequirement Cost => new BuildRequirement { wood = 0, gold = 0 };
    public override IReadOnlyList<string> Actions => new[] { "nuevo obrero" };
}

public class TownhallLevel2 : TownhallLevel1
{
    public override int Level => 2;
    public override BuildRequirement Cost => new BuildRequirement { wood = 50, gold = 50 };
    public override IReadOnlyList<string> Actions => new[] { "nuevo obrero", "nuevo marino" };
}

public class TownhallLevel3 : TownhallLevel2
{
    public override int Level => 3;
    public override BuildRequirement Cost => new BuildRequirement { wood = 100, gold = 100 };
    public override IReadOnlyList<string> Actions => new[] { "nuevo obrero", "nuevo marino", "nuevo espia", "nuevo experto" };
}

public class TownhallLevel4 : TownhallLevel3
{
    public override int Level => 4;
    public override BuildRequirement Cost => new BuildRequirement { wood = 150, gold = 150 };
    public override IReadOnlyList<string> Actions => new[] { "nuevo obrero", "nuevo marino", "nuevo espia", "nuevo experto" };
}
#endregion

#region Barracks
public class BarracksLevel1 : Building
{
    public override string Code => "barracks";
    public override int Level => 1;
    public override BuildRequirement Cost => new BuildRequirement { wood = 30, gold = 10 };
    public override IReadOnlyList<string> Actions => new[] { "nuevo soldado" };
}

public class BarracksLevel2 : BarracksLevel1
{
    public override int Level => 2;
    public override BuildRequirement Cost => new BuildRequirement { wood = 60, gold = 30 };
    public override IReadOnlyList<string> Actions => new[] { "nuevo soldado", "nuevo tanque" };
}

public class BarracksLevel3 : BarracksLevel2
{
    public override int Level => 3;
    public override BuildRequirement Cost => new BuildRequirement { wood = 90, gold = 60 };
    public override IReadOnlyList<string> Actions => new[] { "nuevo soldado", "nuevo tanque", "nuevo elite" };
}

public class BarracksLevel4 : BarracksLevel3
{
    public override int Level => 4;
    public override BuildRequirement Cost => new BuildRequirement { wood = 120, gold = 90 };
    public override IReadOnlyList<string> Actions => new[] { "nuevo soldado", "nuevo tanque", "nuevo elite", "nuevo destructor" };
}
#endregion

#region Airport
public class AirportLevel1 : Building
{
    public override string Code => "airport";
    public override int Level => 1;
    public override BuildRequirement Cost => new BuildRequirement { wood = 40, gold = 20 };
    public override IReadOnlyList<string> Actions => new[] { "nuevo avion" };
}

public class AirportLevel2 : AirportLevel1
{
    public override int Level => 2;
    public override BuildRequirement Cost => new BuildRequirement { wood = 80, gold = 40 };
    public override IReadOnlyList<string> Actions => new[] { "nuevo avion", "nuevo bombardero" };
}
#endregion

#region Dock
public class DockLevel1 : Building
{
    public override string Code => "dock";
    public override int Level => 1;
    public override BuildRequirement Cost => new BuildRequirement { wood = 25, gold = 10 };
    public override IReadOnlyList<string> Actions => new[] { "nuevo pesquero" };
}

public class DockLevel2 : DockLevel1
{
    public override int Level => 2;
    public override BuildRequirement Cost => new BuildRequirement { wood = 50, gold = 30 };
    public override IReadOnlyList<string> Actions => new[] { "nuevo pesquero", "nuevo mercante" };
}

public class DockLevel3 : DockLevel2
{
    public override int Level => 3;
    public override BuildRequirement Cost => new BuildRequirement { wood = 75, gold = 60 };
    public override IReadOnlyList<string> Actions => new[] { "nuevo pesquero", "nuevo mercante", "nuevo patrulla" };
}

public class DockLevel4 : DockLevel3
{
    public override int Level => 4;
    public override BuildRequirement Cost => new BuildRequirement { wood = 100, gold = 100 };
    public override IReadOnlyList<string> Actions => new[] { "nuevo pesquero", "nuevo mercante", "nuevo patrulla", "nuevo portavion" };
}
#endregion

#region Hut
public class HutLevel1 : Building
{
    public override string Code => "hut";
    public override int Level => 1;
    public override BuildRequirement Cost => new BuildRequirement { wood = 10 };
    public override IReadOnlyList<string> Actions => new[] { "otorga 4 lugares" };
}

public class HutLevel2 : HutLevel1
{
    public override int Level => 2;
    public override BuildRequirement Cost => new BuildRequirement { wood = 20 };
    public override IReadOnlyList<string> Actions => new[] { "otorga 8 lugares" };
}

public class HutLevel3 : HutLevel2
{
    public override int Level => 3;
    public override BuildRequirement Cost => new BuildRequirement { wood = 30 };
    public override IReadOnlyList<string> Actions => new[] { "otorga 16 lugares" };
}

public class HutLevel4 : HutLevel3
{
    public override int Level => 4;
    public override BuildRequirement Cost => new BuildRequirement { wood = 40 };
    public override IReadOnlyList<string> Actions => new[] { "otorga 32 lugares" };
}
#endregion

#region Farm
public class FarmLevel1 : Building
{
    public override string Code => "farm";
    public override int Level => 1;
    public override BuildRequirement Cost => new BuildRequirement { wood = 20 };
    public override IReadOnlyList<string> Actions => new[] { "genera 4 de comida" };
}

public class FarmLevel2 : FarmLevel1
{
    public override int Level => 2;
    public override BuildRequirement Cost => new BuildRequirement { wood = 40 };
    public override IReadOnlyList<string> Actions => new[] { "genera 8 de comida" };
}

public class FarmLevel3 : FarmLevel2
{
    public override int Level => 3;
    public override BuildRequirement Cost => new BuildRequirement { wood = 60 };
    public override IReadOnlyList<string> Actions => new[] { "genera 16 de comida" };
}

public class FarmLevel4 : FarmLevel3
{
    public override int Level => 4;
    public override BuildRequirement Cost => new BuildRequirement { wood = 80 };
    public override IReadOnlyList<string> Actions => new[] { "genera 32 de comida" };
}
#endregion

#region Academy
public class AcademyLevel1 : Building
{
    public override string Code => "academy";
    public override int Level => 1;
    public override BuildRequirement Cost => new BuildRequirement { wood = 30, gold = 20 };
    public override IReadOnlyList<string> Actions => new[] { "nuevo aprendiz" };
}

public class AcademyLevel2 : AcademyLevel1
{
    public override int Level => 2;
    public override BuildRequirement Cost => new BuildRequirement { wood = 60, gold = 40 };
    public override IReadOnlyList<string> Actions => new[] { "nuevo aprendiz", "nuevo medico" };
}

public class AcademyLevel3 : AcademyLevel2
{
    public override int Level => 3;
    public override BuildRequirement Cost => new BuildRequirement { wood = 90, gold = 60 };
    public override IReadOnlyList<string> Actions => new[] { "nuevo aprendiz", "nuevo medico", "nuevo erudito" };
}

public class AcademyLevel4 : AcademyLevel3
{
    public override int Level => 4;
    public override BuildRequirement Cost => new BuildRequirement { wood = 120, gold = 80 };
    public override IReadOnlyList<string> Actions => new[] { "nuevo aprendiz", "nuevo medico", "nuevo erudito", "nuevo cientifico" };
}
#endregion

#region Atalaya
public class AtalayaLevel1 : Building
{
    public override string Code => "atalaya";
    public override int Level => 1;
    public override BuildRequirement Cost => new BuildRequirement { wood = 20 };
    public override IReadOnlyList<string> Actions => new[] { "Defensa 4 disparos por segundo" };
}

public class AtalayaLevel2 : AtalayaLevel1
{
    public override int Level => 2;
    public override BuildRequirement Cost => new BuildRequirement { wood = 40 };
    public override IReadOnlyList<string> Actions => new[] { "Defensa 8 disparos por segundo" };
}

public class AtalayaLevel3 : AtalayaLevel2
{
    public override int Level => 3;
    public override BuildRequirement Cost => new BuildRequirement { wood = 60 };
    public override IReadOnlyList<string> Actions => new[] { "Defensa 16 disparos por segundo" };
}

public class AtalayaLevel4 : AtalayaLevel3
{
    public override int Level => 4;
    public override BuildRequirement Cost => new BuildRequirement { wood = 80 };
    public override IReadOnlyList<string> Actions => new[] { "Defensa 32 disparos por segundo" };
}
#endregion

#region Wall
public class WallLevel1 : Building
{
    public override string Code => "wall";
    public override int Level => 1;
    public override BuildRequirement Cost => new BuildRequirement { wood = 10 };
    public override IReadOnlyList<string> Actions => new[] { "resistencia 10" };
}

public class WallLevel2 : WallLevel1
{
    public override int Level => 2;
    public override BuildRequirement Cost => new BuildRequirement { wood = 20 };
    public override IReadOnlyList<string> Actions => new[] { "resistencia 20" };
}

public class WallLevel3 : WallLevel2
{
    public override int Level => 3;
    public override BuildRequirement Cost => new BuildRequirement { wood = 30 };
    public override IReadOnlyList<string> Actions => new[] { "resistencia 30" };
}

public class WallLevel4 : WallLevel3
{
    public override int Level => 4;
    public override BuildRequirement Cost => new BuildRequirement { wood = 40 };
    public override IReadOnlyList<string> Actions => new[] { "resistencia 40" };
}
#endregion

#region Sawmill
public class SawmillLevel1 : Building
{
    public override string Code => "lumbermill";
    public override int Level => 1;
    public override BuildRequirement Cost => new BuildRequirement { wood = 15, gold = 5 };
    public override IReadOnlyList<string> Actions => new[] { "potencia madera por 5" };
}

public class SawmillLevel2 : SawmillLevel1
{
    public override int Level => 2;
    public override BuildRequirement Cost => new BuildRequirement { wood = 30, gold = 10 };
    public override IReadOnlyList<string> Actions => new[] { "potencia madera por 15" };
}

public class SawmillLevel3 : SawmillLevel2
{
    public override int Level => 3;
    public override BuildRequirement Cost => new BuildRequirement { wood = 45, gold = 20 };
    public override IReadOnlyList<string> Actions => new[] { "potencia madera por 30" };
}
#endregion

#region CronoExtractor
public class CronoExtractorLevel1 : Building
{
    public override string Code => "extractor";
    public override int Level => 1;
    public override BuildRequirement Cost => new BuildRequirement { gold = 20, wood = 20, sciencePoints = 10 };
    public override IReadOnlyList<string> Actions => new[] { "Nuevo hechicero", "Asistencia futuro" };
}

public class CronoExtractorLevel2 : CronoExtractorLevel1
{
    public override int Level => 2;
    public override BuildRequirement Cost => new BuildRequirement { gold = 40, wood = 40, sciencePoints = 20 };
    public override IReadOnlyList<string> Actions => new[] { "Nuevo hechicero", "Asistencia futuro", "Misiones pasado" };
}

public class CronoExtractorLevel3 : CronoExtractorLevel2
{
    public override int Level => 3;
    public override BuildRequirement Cost => new BuildRequirement { gold = 60, wood = 60, sciencePoints = 30 };
    public override IReadOnlyList<string> Actions => new[] { "Nuevo hechicero", "Asistencia futuro", "Misiones pasado", "proteccion" };
}

public class CronoExtractorLevel4 : CronoExtractorLevel3
{
    public override int Level => 4;
    public override BuildRequirement Cost => new BuildRequirement { gold = 80, wood = 80, sciencePoints = 40 };
    public override IReadOnlyList<string> Actions => new[] { "Nuevo hechicero", "Asistencia futuro", "Misiones pasado", "proteccion", "envio tabla" };
}
#endregion

