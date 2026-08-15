using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// [GPX-DOC-v1] ================================================================================
// Stock agregado de beam blank: teorico, real, asignado y libre, en toneladas y unidades.
// ================================================================================================

namespace GPX.Negocio.ORM
{
    /// <summary>
    /// Clase StockBeamBlank. Stock agregado de beam blank: teorico, real, asignado y libre, en toneladas y
    /// unidades.
    /// </summary>
    public class StockBeamBlank
    {
        public int st_IdRegistro { get; set; } = 0;
        public string st_sociedad { get; set; } = "";
        public string st_tipo_semi { get; set; } = "";
        public string st_calidad { get; set; } = "";
        public int st_longitud { get; set; } = 0;
        public decimal st_stock_tn_teorico { get; set; } = 0;
        public decimal st_stock_tn { get; set; } = 0;
        public int st_stock_uds { get; set; } = 0;
        public decimal st_asignadas_tn { get; set; } = 0;
        public int st_asignadas_uds { get; set; } = 0;
        public decimal st_libres_tn { get; set; } = 0;
        public int st_libres_uds { get; set; } = 0;
        public decimal st_PesoSemi { get; set; } = 0;
    }
}
