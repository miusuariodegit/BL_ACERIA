
using GPX.Negocio.COP;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// [GPX-DOC-v1] ================================================================================
// Entidad del calendario de fusion: sociedad, centro, fecha y franjas horarias (hasta 16) programadas.
// ================================================================================================

namespace GPX.Negocio.ORM
{
    /// <summary>
    /// Clase CalendarioFusionHorno. Entidad del calendario de fusion: sociedad, centro, fecha y franjas
    /// horarias (hasta 16) programadas.
    /// </summary>
    public class CalendarioFusionHorno
    {
        public long cafId { get; set; } = 0;
        public string cafSociedad { get; set; } = "";
        public int cafIdCentro { get; set; } = 0;
        public DateTime cafFecha { get; set; } = Constantes.FechaGlobal;
        public bool cafHora1 { get; set; } = false;
        public bool cafHora2 { get; set; } = false;
        public bool cafHora3 { get; set; } = false;
        public bool cafHora4 { get; set; } = false;
        public bool cafHora5 { get; set; } = false;
        public bool cafHora6 { get; set; } = false;
        public bool cafHora7 { get; set; } = false;
        public bool cafHora8 { get; set; } = false;
        public bool cafHora9 { get; set; } = false;
        public bool cafHora10 { get; set; } = false;
        public bool cafHora11 { get; set; } = false;
        public bool cafHora12 { get; set; } = false;
        public bool cafHora13 { get; set; } = false;
        public bool cafHora14 { get; set; } = false;
        public bool cafHora15 { get; set; } = false;
        public bool cafHora16 { get; set; } = false;
        public bool cafHora17 { get; set; } = false;
        public bool cafHora18 { get; set; } = false;
        public bool cafHora19 { get; set; } = false;
        public bool cafHora20 { get; set; } = false;
        public bool cafHora21 { get; set; } = false;
        public bool cafHora22 { get; set; } = false;
        public bool cafHora23 { get; set; } = false;
        public bool cafHora24 { get; set; } = false;

        public string TipoSemiHora1 { get; set; } = "";
        public string TipoSemiHora2 { get; set; } = "";
        public string TipoSemiHora3 { get; set; } = "";
        public string TipoSemiHora4 { get; set; } = "";
        public string TipoSemiHora5 { get; set; } = "";
        public string TipoSemiHora6 { get; set; } = "";
        public string TipoSemiHora7 { get; set; } = "";
        public string TipoSemiHora8 { get; set; } = ""; 
        public string TipoSemiHora9 { get; set; } = "";                       
        public string TipoSemiHora10 { get; set; } = "";
        public string TipoSemiHora11 { get; set; } = "";
        public string TipoSemiHora12 { get; set; } = "";
        public string TipoSemiHora13 { get; set; } = "";
        public string TipoSemiHora14 { get; set; } = "";
        public string TipoSemiHora15 { get; set; } = "";
        public string TipoSemiHora16 { get; set; } = "";
        public string TipoSemiHora17 { get; set; } = "";
        public string TipoSemiHora18 { get; set; } = "";
        public string TipoSemiHora19 { get; set; } = "";
        public string TipoSemiHora20 { get; set; } = "";
        public string TipoSemiHora21 { get; set; } = "";
        public string TipoSemiHora22 { get; set; } = "";
        public string TipoSemiHora23 { get; set; } = "";
        public string TipoSemiHora24 { get; set; } = "";
    }
}
