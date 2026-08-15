
using GPX.Negocio.COP;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// [GPX-DOC-v1] ================================================================================
// Registro de coladas cargadas: fabricacion, colada, calidad/tipo/longitud del semielaborado, unidades
// cargadas, fecha y usuario.
// ================================================================================================

namespace GPX.Negocio.ORM
{
    /// <summary>
    /// Clase ControlColadasCargadas. Registro de coladas cargadas: fabricacion, colada,
    /// calidad/tipo/longitud del semielaborado, unidades cargadas, fecha y usuario.
    /// </summary>
    public class ControlColadasCargadas
    {
        public int IdFabGpb { get; set; } = 0;
        public int IdColada { get; set; } = 0;
        public string CalidadSemi { get; set; } = string.Empty;
        public string TipoSemi { get; set; } = string.Empty;
        public int LongitudSemi { get; set; } = 0;
        public int UnCargadas { get; set; } = 0;
        public DateTime FechaCarga { get; set; } = Constantes.FechaGlobal;
        public string UsrCargo { get; set; } = string.Empty;
        public string IdTemporalFam { get; set; } = string.Empty;
        public string OndenFab { get; set; } = string.Empty;


    }
}
