using System;
using System.Collections.Generic;
using System.Text;

// [GPX-DOC-v1] ================================================================================
// Modelo de vista de calidades por colada: calidad, coladas requeridas/repartidas y si es calidad
// estandar.
// ================================================================================================

namespace GPX.Negocio.Aceria
{
    /// <summary>
    /// Clase ListaCalidadesXColada. Modelo de vista de calidades por colada: calidad, coladas
    /// requeridas/repartidas y si es calidad estandar.
    /// </summary>
    public class ListaCalidadesXColada
    {
        public int OrdenCalidad { get; set; } = 0;
        public string Calidad { get; set; } = string.Empty;
        public int ColadasRequeridas { get; set; } = 0;
        public int ColadasRepartidas { get; set; } = 0;
        public bool EsCalidadEstandar { get; set; } = false;
        public int ColadasRestantes
        {
            get
            {
                return ColadasRequeridas - ColadasRepartidas;
            }
        }
    }
}
