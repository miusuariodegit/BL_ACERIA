using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// [GPX-DOC-v1] ================================================================================
// Entidad tundish: sociedad, necesidad por tipo de BB, si esta activo, tipo estandar y hora real de
// cierre de cada boca.
// ================================================================================================

namespace GPX.Negocio.ORM
{
    /// <summary>
    /// Clase Tundish. Entidad tundish: sociedad, necesidad por tipo de BB, si esta activo, tipo estandar y
    /// hora real de cierre de cada boca.
    /// </summary>
    public class Tundish
    {
        public int tuID { get; set; }
        public string tuSociedad { get; set; }
        public decimal tuNecesidadBB1 { get; set; }
        public decimal tuNecesidadBB2 { get; set; }
        public decimal tuNecesidadBB3 { get; set; }
        public bool tuActivo { get; set; }
        public int tuTipoStandard { get; set; }
        public string tuCierre1Real { get; set; }
        public string tuCierre2Real { get; set; }
        public string tuCierre3Real { get; set; }
        public string tuCierre4Real { get; set; }
        public string tuCierre5Real { get; set; }
        public string tuCierre6Real { get; set; }
    }
}
