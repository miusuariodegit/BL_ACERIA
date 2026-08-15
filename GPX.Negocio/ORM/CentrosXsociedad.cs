using System;
using System.Collections.Generic;
using System.Text;

// [GPX-DOC-v1] ================================================================================
// Catalogo de centros productivos por sociedad, con codigos SAP/GESAC, rendimiento y merma.
// ================================================================================================

namespace GPX.Negocio.ORM
{
    /// <summary>
    /// Clase CentrosXsociedad. Catalogo de centros productivos por sociedad, con codigos SAP/GESAC,
    /// rendimiento y merma.
    /// </summary>
    public class CentrosXsociedad
    {
        public int csID { get; set; }
        public string? csCodSociedad { get; set; }
        public string? csCodCentro { get; set; }
        public string? csCodSAP { get; set; }
        public string? csCodGESAC { get; set; }
        public string? csNombre { get; set; }
        public bool csActivo { get; set; }
        public decimal csRendimiento { get; set; }
        public decimal csMerma { get; set; }

    }
}
