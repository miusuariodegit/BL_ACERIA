using GPX.Negocio.COP;
using System;
using System.Collections.Generic;
using System.Text;

// [GPX-DOC-v1] ================================================================================
// Par fecha/valor usado en la carga masiva del calendario de fusion desde archivo.
// ================================================================================================

namespace GPX.Negocio.Aceria
{
    /// <summary>
    /// Clase ListaCargaMasivaCalendario. Par fecha/valor usado en la carga masiva del calendario de fusion
    /// desde archivo.
    /// </summary>
    public class ListaCargaMasivaCalendario
    {
        /// <summary>
        /// Inicializa una nueva instancia de la clase ListaCargaMasivaCalendario.
        /// </summary>
        public ListaCargaMasivaCalendario(DateTime fecha, int valor)
        {
            this.fecha = fecha;
            this.valor = valor;
        }

        public DateTime fecha { get; set; } = Constantes.FechaGlobal;
        public int valor { get; set; } = 0;
    }
}
