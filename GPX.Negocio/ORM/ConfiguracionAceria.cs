using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// [GPX-DOC-v1] ================================================================================
// Parametros de configuracion de una aceria por sociedad/maquina: tiempos de colada, vida util de
// tundish, pesos y velocidades por tipo de BB, limites de calidades estandar.
// ================================================================================================

namespace GPX.Negocio.ORM
{
    /// <summary>
    /// Clase ConfiguracionAceria. Parametros de configuracion de una aceria por sociedad/maquina: tiempos
    /// de colada, vida util de tundish, pesos y velocidades por tipo de BB, limites de calidades estandar.
    /// </summary>
    public class ConfiguracionAceria
    {
        public int caId { get; set; }  
        public string caCodSociedad { get; set; }
        public int caCodMaquina { get; set; }
        public int caToneladasXCuchara { get; set; }
        public int caTiempoMinColada { get; set; }
        public int caTiempoMaxColada { get; set; }
        public int caTiempoLF { get; set; }
        public int caMaximoPerfiles { get; set; }
        public int caMaxPerfilesXTipo { get; set; }
        public decimal caVidaUtilTundish { get; set; }
        public decimal caPesoLinealBB1 { get; set; }
        public decimal caPesoLinealBB2 { get; set; }
        public decimal caPesoLinealBB3 { get; set; }
        public decimal caVelocidadMaximaBB1 { get; set; }
        public decimal caVelocidadMaximaBB2 { get; set; }
        public decimal caVelocidadMaximaBB3 { get; set; }
        public int caMaximoFabricacionesEstandar { get; set; }
        public int caColadasCalidadEstandarMinimas { get; set; }
        public int caColadasCalidadEstandarMaximas { get; set; }
        public int caMinutosCambioTundish { get; set; }
        public int caColadasCalidadEspecialMinimas { get; set; }
        public int caColadasCalidadEspecialMaximas { get; set; }
        public decimal caTnxHoraBB1 { get; set; }
        public decimal caTnxHoraBB2 { get; set; }
        public decimal caTnxHoraBB3 { get; set; }
        public int caCargaMasivaCalendarioValor { get; set; }
    }
}
