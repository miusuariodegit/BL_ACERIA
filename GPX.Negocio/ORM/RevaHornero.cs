
using GPX.Negocio.COP;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

// [GPX-DOC-v1] ================================================================================
// Registro de revision hornero: fabricacion, fecha/semana prevista de fin, orden de mezcla, sociedad,
// maquina, estatus y material.
// ================================================================================================

namespace GPX.Negocio.ORM
{
    /// <summary>
    /// Clase RevaHornero. Registro de revision hornero: fabricacion, fecha/semana prevista de fin, orden de
    /// mezcla, sociedad, maquina, estatus y material.
    /// </summary>
    public  class RevaHornero
    {

        public int ReHIdRegistro { get; set; } = 0;
        public int ReHdFab { get; set; } = 0;
        public DateTime  ReHFechaPrevFin { get; set; } = Constantes.FechaGlobal;    
        public int ReHSemanaPrevFin { get; set; } = 0;
        public string ReHOrdenMix { get; set; } = "";
        public string ReHNomFamCont { get; set; } = "";
        public int ReHNumCiclo { get; set; } = 0;
        public string ReHSociedad { get; set; } = "";
        public int ReHIdMaquina { get; set; } = 0;
        public int ReHEstatus { get; set; } = 0;
        public int ReHOrdenMaquina { get; set; } = 0;
        public string ReHFamiliaMaterial { get; set; } = "";
        public string ReHEstadoCargaImagen { get; set; } = "";
        public string ReHIdMaterial { get; set; } = "";
        public string ReHNombreMaterial { get; set; } = "";
        public string ReHCalidadMaterial { get; set; } = "";
        public int ReHLongitudMaterial { get; set; } = 0;
        public string ReHTipoSemi { get; set; } = "";
        public string ReHCalidadSemi { get; set; } = "";
        public int ReHLongitudSemi { get; set; } = 0;
        public int ReHUnAsignadas { get; set; } = 0;
        public int ReHPesoSemi { get; set; } = 0;   
        public decimal ReHToneladasBrutas { get; set; } = 0;
        public decimal ReHTnCargadas { get; set; } = 0;
        public int ReHUnCargadas { get; set; } = 0;
        public string ReOrdenFab { get; set; } = "";



    }
}
