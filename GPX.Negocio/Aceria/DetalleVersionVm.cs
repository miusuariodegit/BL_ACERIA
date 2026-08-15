using System;
using System.Collections.Generic;
using System.Text;

// [GPX-DOC-v1] ================================================================================
// Modelo de vista del detalle de una version de tundish: tipo de semielaborado, barras, longitud,
// calidad, fecha/semana prevista y GAP.
// ================================================================================================

namespace GPX.Negocio.Aceria
{
    /// <summary>
    /// Clase DetalleVersionVm. Modelo de vista del detalle de una version de tundish: tipo de
    /// semielaborado, barras, longitud, calidad, fecha/semana prevista y GAP.
    /// </summary>
    public  class DetalleVersionVm
    {
        public string IdDetalle { get; set; } = string.Empty;
        public string TipoSemi { get; set; } = string.Empty;
        public int NumeroBarras { get; set; }
        public int Longitud { get; set; }
        public string Calidad { get; set; } = string.Empty;
        public DateTime FechaPrevIni { get; set; }
        public string? SemanaPrevIni { get; set; }

        public Boolean GAP { get; set; } = false;
    }
}
