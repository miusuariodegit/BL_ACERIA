using Dapper;
using GPX.Negocio.ORM;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading.Tasks;

// [GPX-DOC-v1] ================================================================================
// Servicio de negocio del calendario de fusion de horno: consulta, actualizacion de un dia u hora
// concreta, y limpieza de la programacion.
// ================================================================================================

namespace GPX.Negocio.Aceria
{
    /// <summary>
    /// Clase CalendarioFusionHornoService. Servicio de negocio del calendario de fusion de horno: consulta,
    /// actualizacion de un dia u hora concreta, y limpieza de la programacion.
    /// </summary>
    public  class CalendarioFusionHornoService
    {

        private readonly string cnn;

        /// <summary>
        /// Inicializa una nueva instancia de la clase CalendarioFusionHornoService.
        /// </summary>
        public CalendarioFusionHornoService(IConfiguration configuration)
        {
            cnn = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("No se encontró la cadena de conexión 'DefaultConnection'.");
        }



        /// <summary>
        /// Obtiene el calendario de fusion de horno hasta la fecha indicada.
        /// </summary>
        public async Task<List<CalendarioFusionHorno>> DameCalendarioFusionHastaAsync(DateTime fechaFin)
        {
            try
            {
                using (var db = new SqlConnection(cnn))
                {
                    var resultado = await db.QueryAsync<CalendarioFusionHorno>(
                        sql: "sp_DameCalendarioFusionHornoHasta", param: new
                        {
                            fechaFin
                        }, commandType: CommandType.StoredProcedure
                        );

                    return resultado.AsList();
                }
            }
            catch (Exception ex)
            {

                throw new Exception("Error al ejecutar sp_DameCalendarioFusionHornoHasta , detalle: \n" + ex.Message, ex);
            }
        }

        /// <summary>
        /// Actualiza el tipo de semielaborado programado para un dia completo del calendario de fusion.
        /// </summary>
        public async Task<Boolean> ActualizaSoloDiaFusionAsync(DateTime diaCalendario, string tipoBB)
        {
            try
            {
                using (var db = new SqlConnection(cnn))
                {
                    await db.ExecuteAsync(sql: "sp_ActualizaSoloDiaFusion", param: new
                    {
                        diaCalendario,
                        tipoBB
                    }, commandType: CommandType.StoredProcedure);
                }

                return true;
            }
            catch (Exception ex)
            {
                throw new Exception("Error al ejecutar sp_ActualizaSoloDiaFusion, detalle: \n" + ex.Message, ex);
            }

        }


        /// <summary>
        /// Actualiza el tipo de semielaborado programado para una hora concreta de un dia del calendario.
        /// </summary>
        public async Task<Boolean> ActualizaSoloDiaHoraFusionAsync(DateTime diaCalendario, int horaCalendario, string tipoBB)
        {
            try
            {
                using (var db = new SqlConnection(cnn))
                {
                    await db.ExecuteAsync(sql: "sp_ActualizaSoloDiaHoraFusion", param: new
                    {
                        diaCalendario,
                        horaCalendario,
                        tipoBB
                    }, commandType: CommandType.StoredProcedure);
                }

                return true;
            }
            catch (Exception ex)
            {
                throw new Exception("Error al ejecutar sp_ActualizaSoloDiaHoraFusion, detalle: \n" + ex.Message, ex);
            }

        }


        /// <summary>
        /// Limpia toda la programacion de fusion de un dia del calendario.
        /// </summary>
        public async Task<Boolean> LimpiarDiaCompletoFusionAsync(DateTime diaCalendario)
        {
            try
            {
                using (var db = new SqlConnection(cnn))
                {
                    await db.ExecuteAsync(sql: "sp_LimipiarDiaCompletoFusion", param: new
                    {
                        diaCalendario

                    }, commandType: CommandType.StoredProcedure);
                }

                return true;
            }
            catch (Exception ex)
            {
                throw new Exception("Error al ejecutar sp_LimipiarDiaCompletoFusion, detalle: \n" + ex.Message, ex);
            }
        }



        /// <summary>
        /// Limpia la programacion de fusion a partir de una hora de inicio dada en un dia del calendario.
        /// </summary>
        public async Task<Boolean> LimpiarApartiDeHoraInicioFusionAsync(DateTime diaCalendario)
        {
            try
            {
                using (var db = new SqlConnection(cnn))
                {
                    await db.ExecuteAsync(sql: "sp_LimipiarDiaCompletoFusionXHoraInicio", param: new
                    {
                        diaCalendario

                    }, commandType: CommandType.StoredProcedure);
                }

                return true;
            }
            catch (Exception ex)
            {
                throw new Exception("Error al ejecutar sp_LimipiarDiaCompletoFusionXHoraInicio, detalle: \n" + ex.Message, ex);
            }
        }

        /// <summary>
        /// Actualiza de una sola vez todos los valores de un registro completo del calendario de fusion.
        /// </summary>
        public async Task<Boolean> ActualizaDiaCompletoFusionAsync(CalendarioFusionHorno values)
        {
            try
            {
                using (var db = new SqlConnection(cnn))
                {
                    await db.ExecuteAsync(sql: "sp_CargaDiaCompletoFusion", param: new
                    {
                        values.cafId,
                        values.cafSociedad,
                        values.cafIdCentro,
                        values.cafFecha,
                        values.cafHora1,
                        values.cafHora2,
                        values.cafHora3,
                        values.cafHora4,
                        values.cafHora5,
                        values.cafHora6,
                        values.cafHora7,
                        values.cafHora8,
                        values.cafHora9,
                        values.cafHora10,
                        values.cafHora11,
                        values.cafHora12,
                        values.cafHora13,
                        values.cafHora14,
                        values.cafHora15,
                        values.cafHora16,
                        values.cafHora17,
                        values.cafHora18,
                        values.cafHora19,
                        values.cafHora20,
                        values.cafHora21,
                        values.cafHora22,
                        values.cafHora23,
                        values.cafHora24,
                        values.TipoSemiHora1,
                        values.TipoSemiHora2,
                        values.TipoSemiHora3,
                        values.TipoSemiHora4,
                        values.TipoSemiHora5,
                        values.TipoSemiHora6,
                        values.TipoSemiHora7,
                        values.TipoSemiHora8,
                        values.TipoSemiHora9,
                        values.TipoSemiHora10,
                        values.TipoSemiHora11,
                        values.TipoSemiHora12,
                        values.TipoSemiHora13,
                        values.TipoSemiHora14,
                        values.TipoSemiHora15,
                        values.TipoSemiHora16,
                        values.TipoSemiHora17,
                        values.TipoSemiHora18,
                        values.TipoSemiHora19,
                        values.TipoSemiHora20,
                        values.TipoSemiHora21,
                        values.TipoSemiHora22,
                        values.TipoSemiHora23,
                        values.TipoSemiHora24


                    }, commandType: CommandType.StoredProcedure);
                }

                return true;
            }
            catch (Exception ex)
            {
                throw new Exception("Error al ejecutar sp_CargaDiaCompletoFusion, detalle: \n" + ex.Message, ex);
            }
        }




    }
}
