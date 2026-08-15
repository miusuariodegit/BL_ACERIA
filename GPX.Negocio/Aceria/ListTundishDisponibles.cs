using GPX.Negocio.COP;
using System;
using System.Collections.Generic;
using System.Text;

// [GPX-DOC-v1] ================================================================================
// Modelo de vista de un tundish disponible: horario, vida util, tipo de semielaborado, estandar
// seleccionado, totales por tipo de BB y estado de cada boca.
// ================================================================================================

namespace GPX.Negocio.Aceria
{
    /// <summary>
    /// Clase ListTundishDisponibles. Modelo de vista de un tundish disponible: horario, vida util, tipo de
    /// semielaborado, estandar seleccionado, totales por tipo de BB y estado de cada boca.
    /// </summary>
    public class ListTundishDisponibles
    {

        public int NumTundish { get; set; } = 0;
        public DateTime FechaCalendario { get; set; } = Constantes.FechaGlobal;
        public int HoraInicio { get; set; } = 0;
        public int HoraFin { get; set; } = 0;
        public int HorasVida { get; set; } = 0;
        public DateTime FechaInicio { get; set; } = Constantes.FechaGlobal;
        public DateTime FechaFin { get; set; } = Constantes.FechaGlobal;

        public string TipoSemi { get; set; } = string.Empty;

        // propiedades para almacenar los datos adicionaes y estados del tundish y no perderlos
        public string NombreTundish { get; set; } = string.Empty;
        public ListTundishStandard? EstandardSeleccionado { get; set; } = null;
        public int tstotalBB1 { get; set; } = 0;
        public int tstotalBB2 { get; set; } = 0;
        public int tstotalBB3 { get; set; } = 0;
        public String StatusBoca1 { get; set; } = string.Empty;
        public String StatusBoca2 { get; set; } = string.Empty;
        public String StatusBoca3 { get; set; } = string.Empty;
        public String StatusBoca4 { get; set; } = string.Empty;
        public String StatusBoca5 { get; set; } = string.Empty;
        public String StatusBoca6 { get; set; } = string.Empty;





        public String tsCierre1 { get; set; } = string.Empty;
        public String tsCierre2 { get; set; } = string.Empty;
        public String tsCierre3 { get; set; } = string.Empty;
        public String tsCierre4 { get; set; } = string.Empty;
        public String tsCierre5 { get; set; } = string.Empty;
        public String tsCierre6 { get; set; } = string.Empty;




        public decimal MinutosXColada { get; set; } = 0;
        public decimal TnXColadaBB1 { get; set; } = 0;
        public decimal TnXColadaBB2 { get; set; } = 0;
        public decimal TnXColadaBB3 { get; set; } = 0;

        public decimal NumColadas { get; set; } = 0;
        public decimal ColadasReales { get; set; } = 0;

        public Boolean Restablacer { get; set; } = false;

        public Boolean TundishActivo { get; set; } = true;


        public decimal TnXLineaBB1 => tstotalBB1 == 0 ? 0 : TnXColadaBB1 / tstotalBB1;
        public decimal TnXLineaBB2 => tstotalBB2 == 0 ? 0 : TnXColadaBB2 / tstotalBB2;
        public decimal TnXLineaBB3 => tstotalBB3 == 0 ? 0 : TnXColadaBB3 / tstotalBB3;



    }
}
