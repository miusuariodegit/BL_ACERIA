using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// [GPX-DOC-v1] ================================================================================
// Estandar de tundish: horas de cierre por boca, si esta activo, prioridad y totales por tipo de BB.
// ================================================================================================

namespace GPX.Negocio.ORM
{
    /// <summary>
    /// Clase TundishStandard. Estandar de tundish: horas de cierre por boca, si esta activo, prioridad y
    /// totales por tipo de BB.
    /// </summary>
    public class TundishStandard
    {
        public int tsId { get; set; } = 0;
        public string tsSociedad { get; set; } = "";
        public string tsCierre1 { get; set; } = "";
        public string tsCierre2 { get; set; } = "";
        public string tsCierre3 { get; set; } = "";
        public string tsCierre4 { get; set; } = "";
        public string tsCierre5 { get; set; } = "";
        public string tsCierre6 { get; set; } = "";
        public bool tsActivo { get; set; } = false;
        public int tsPrioridad { get; set; } = 0;
        public int tstotalBB1 { get; set; } = 0;
        public int tstotalBB2 { get; set; } = 0;
        public int tstotalBB3 { get; set; } = 0;
    }
}
