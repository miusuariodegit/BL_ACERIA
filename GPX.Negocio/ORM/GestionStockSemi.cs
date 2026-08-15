
using GPX.Negocio.COP;
using System;

// [GPX-DOC-v1] ================================================================================
// Movimiento de stock de semielaborado: tipo de operacion, toneladas y unidades
// brutas/stock/asignadas/libres.
// ================================================================================================

namespace GPX.Negocio.ORM
{
    /// <summary>
    /// Clase GestionStockSemi. Movimiento de stock de semielaborado: tipo de operacion, toneladas y
    /// unidades brutas/stock/asignadas/libres.
    /// </summary>
    public class GestionStockSemi
    {
        public long geId { get; set; } = 0;
        public DateTime geFechaOperacion { get; set; } = Constantes.FechaGlobal;
        public string geTipoOperacion { get; set; } = string.Empty;
        public int geIdFabGPB { get; set; } = 0;
        public string geCalidadSemi { get; set; } = string.Empty;
        public decimal geTnBrutas { get; set; } = 0;
        public decimal geTnStock { get; set; } = 0;
        public decimal geTnAsignadas { get; set; } = 0;
        public decimal geTnAsignadasTotal { get; set; } = 0;
        public decimal geTnLibres { get; set; } = 0;
        public string geFamilia { get; set; } = string.Empty;
        public int geLong { get; set; } = 0;
        public int gelongSemi { get; set; } = 0;
        public decimal gePesoSemi { get; set; } = 0;
        public string geTipoSemi { get; set; } = string.Empty;
        public string geNombreUsuario { get; set; } = string.Empty;
        public int geUnAsignadas { get; set; } = 0;
        public int geUnBrutas { get; set; } = 0;
        public int geUnStock { get; set; } = 0;
        public int geUnAsignadasTotal { get; set; } = 0;
        public int geUnLibres { get; set; } = 0;
        public bool geEsNuevoRegistro { get; set; } = false;
    }
}
