using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// [GPX-DOC-v1] ================================================================================
// Orden de prioridad visual de una calidad en la planificacion (color, color de fuente, orden).
// ================================================================================================

namespace GPX.Negocio.ORM
{
    /// <summary>
    /// Clase OrdenCalidadPlanificacion. Orden de prioridad visual de una calidad en la planificacion
    /// (color, color de fuente, orden).
    /// </summary>
    public class OrdenCalidadPlanificacion
    {
        public string ocpCalidad { get; set; } = string.Empty;
        public string ocpColor { get; set; } = string.Empty;
        public string ocpColorFuente { get; set; } = string.Empty;
        public int ocpOrden { get; set; } = 0;
    }
}
