using GPX.Negocio.ORM;
using System;
using System.Collections.Generic;
using System.Text;

// [GPX-DOC-v1] ================================================================================
// Extiende StockBeamBlank agregando numero de cortes y merma.
// ================================================================================================

namespace GPX.Negocio.Aceria
{
    /// <summary>
    /// Clase ListaStockBeamBlank. Extiende StockBeamBlank agregando numero de cortes y merma.
    /// </summary>
    public class ListaStockBeamBlank: StockBeamBlank
    {
        public int st_NunCortes { get; set; } = 0;
        public decimal st_Merma { get; set; } = 0;
    }
}
