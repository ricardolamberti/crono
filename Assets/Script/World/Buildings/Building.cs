using System.Collections.Generic;
using static DTO;

// Defines horizontal or vertical placement for buildings that span multiple tiles

/// <summary>
/// Clase base para todas las construcciones del juego.
/// </summary>
public abstract class Building
{
    /// <summary>
    /// Codigo identificador de la construccion.
    /// </summary>
    public abstract string Code { get; }

    /// <summary>
    /// Nivel o etapa de evolucion.
    /// </summary>
    public abstract int Level { get; }

    /// <summary>
    /// Requerimientos de recursos para construir.
    /// </summary>
    public abstract BuildRequirement Cost { get; }

    /// <summary>
    /// Radio de visibilidad que despeja la construcción.
    /// </summary>
    public virtual int VisibilityRadius => 2;

    public Orientation Orientation { get; set; } = Orientation.Horizontal;

    /// <summary>
    /// Indicates if the sprite should rotate when the orientation changes.
    /// Bridges don't rotate so they override this.
    /// </summary>
    public virtual bool ShouldRotate => false;

    /// <summary>
    /// Key used to pick the sprite for this building.
    /// </summary>
    public virtual string SpriteKey => $"{Code}_{Level}";

    /// <summary>
    /// Hit points for the structure. If it reaches 0 the building is destroyed.
    /// </summary>
    public virtual int MaxResistance => 10;

    /// <summary>
    /// Damage dealt at long range. Non combat buildings can return 0.
    /// </summary>
    public virtual int RangedDamage => 0;

    /// <summary>
    /// Type of projectile used by the structure.
    /// </summary>
    public virtual WeaponType RangedWeapon => WeaponType.Arrow;

    /// <summary>
    /// Range for the long distance attack.
    /// </summary>
    public virtual int AttackRange => 3;

    /// <summary>
    /// Acciones habilitadas en este nivel.
    /// </summary>
    public abstract IEnumerable<ControlPanelAction> GetActions(MapCellDTO cell);
}
