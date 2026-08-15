using System;
using System.Collections.Generic;
using System.Text;

// [GPX-DOC-v1] ================================================================================
// Modelo de vista del detalle de necesidad de beam blank por orden y calidad: toneladas de necesidad y
// coladas necesarias vs reales.
// ================================================================================================

namespace GPX.Negocio.Aceria
{
    /// <summary>
    /// Clase ListDetalleNecesidadBB. Modelo de vista del detalle de necesidad de beam blank por orden y
    /// calidad: toneladas de necesidad y coladas necesarias vs reales.
    /// </summary>
    public class ListDetalleNecesidadBB
    {
        public int OrdenFab { get; set; } = 0;
        public string Calidad { get; set; } = "";
        public string TipoBB { get; set; } = "";
        public decimal TnNecesidad { get; set; } = 0;
        public decimal ColadasNecesarias { get; set; } = 0;
        public decimal ColadasReales { get; set; } = 0;
        public decimal TnReales { get; set; } = 0;

        public decimal DiferenciaColadas
        {
            get
            {
                return ColadasReales - ColadasNecesarias;
            }
        }

    }
}
