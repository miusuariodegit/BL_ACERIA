using GPX.Negocio.ORM;
using System;
using System.Collections.Generic;
using System.Text;

// [GPX-DOC-v1] ================================================================================
// State container (scoped) que mantiene la version de tundish seleccionada actualmente en la interfaz.
// ================================================================================================

namespace GPX.Negocio.Aceria
{
    /// <summary>
    /// Clase VersionTundishSeleccionadoState. State container (scoped) que mantiene la version de tundish
    /// seleccionada actualmente en la interfaz.
    /// </summary>
    public class VersionTundishSeleccionadoState
    {
        public ConfiguracionTundishControl? VersionTundish { get; set; }
    }
}
