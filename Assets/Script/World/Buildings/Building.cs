using System.Collections.Generic;
using static DTO;

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

    /// <summary>
    /// Acciones habilitadas en este nivel.
    /// </summary>
    public abstract IEnumerable<ControlPanelAction> GetActions(MapCellDTO cell);
}
