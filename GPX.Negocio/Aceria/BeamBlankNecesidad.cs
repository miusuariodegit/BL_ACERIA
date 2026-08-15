using System;
using System.Collections.Generic;
using System.Text;

// [GPX-DOC-v1] ================================================================================
// DTO con la necesidad de beam blank por orden de fabricacion: sociedad, maquina, material, calidad,
// longitud, toneladas/unidades a fabricar y fechas de prevision.
// ================================================================================================

namespace GPX.Negocio.Aceria
{
    /// <summary>
    /// Clase BeamBlankNecesidad. DTO con la necesidad de beam blank por orden de fabricacion: sociedad,
    /// maquina, material, calidad, longitud, toneladas/unidades a fabricar y fechas de prevision.
    /// </summary>
    public class BeamBlankNecesidad
    {

        public int numRegistro { get; set; } = 0;
        public string? Sociedad { get; set; }
        public int CodMaquina { get; set; }
        public int IdFab { get; set; }
        public string? MatSemi { get; set; }
        public string? CalidadSemi { get; set; }
        public int LongitudSemi { get; set; }
        public string? Familia { get; set; }
        public string? Calidad { get; set; }
        public int Longitud { get; set; }
        public decimal TnsAFabSemi { get; set; }
        public int UdsAFabSemi { get; set; }
        public DateTime FechaPrevIni { get; set; }
        public string?   SemanaPrevIni { get; set; }

        public string CalidadSemiLong
        {
            get
            {
                return CalidadSemi ?? string.Empty;
            }
        }

        public string NombreSemana
        {
            get
            {
                return "Semana " + SemanaPrevIni;
            }
        }


    }
}
