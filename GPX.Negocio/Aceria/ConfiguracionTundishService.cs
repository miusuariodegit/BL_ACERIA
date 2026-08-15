using Dapper;
using GPX.Negocio.ORM;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

// [GPX-DOC-v1] ================================================================================
// Servicio de configuracion de tundish: activacion de una version y consulta de versiones por rango de
// fechas.
// ================================================================================================

namespace GPX.Negocio.Aceria
{
    /// <summary>
    /// Clase ConfiguracionTundishService. Servicio de configuracion de tundish: activacion de una version y
    /// consulta de versiones por rango de fechas.
    /// </summary>
    public class ConfiguracionTundishService
    {

        private readonly string cnn;

        /// <summary>
        /// Inicializa una nueva instancia de la clase ConfiguracionTundishService.
        /// </summary>
        public ConfiguracionTundishService(IConfiguration configuration)
        {
            cnn = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("No se encontró la cadena de conexión 'DefaultConnection'.");
        }



        /// <summary>
        /// Marca como activa una version de configuracion de tundish identificada por su Id.
        /// </summary>
        public async Task<Boolean> ActivaVersionTundish(string IdVersion)
        {
            try
            {
                using (var db = new SqlConnection(cnn))
                {
                    await db.ExecuteAsync(sql: "sp_ActivaVersionTundish", param: new
                    {
                        IdVersion,
                    }, commandType: CommandType.StoredProcedure);
                }

                return true;
            }
            catch (Exception ex)
            {
                throw new Exception("Error al ejecutar sp_ActivaVersionTundish, detalle: \n" + ex.Message, ex);
            }

        }



        /// <summary>
        /// Consulta las versiones de configuracion de tundish creadas dentro de un rango de fechas.
        /// </summary>
        public async Task<List<ConfiguracionTundishControl>> CansultaVercionXRango(DateTime FechaInicio, DateTime FechaFin)
        {
            try
            {
                using var db = new SqlConnection(cnn);

                var resultado = await db.QueryAsync<ConfiguracionTundishControl>(
                    sql: "sp_ConsultaConfiguracionTundishControlxRango",
                    param: new
                    {
                        FechaInicio,
                        FechaFin
                    },
                    commandType: CommandType.StoredProcedure
                );

                return resultado.ToList();
            }
            catch (Exception ex)
            {
                throw new Exception("Error al ejecutar sp_ConsultaConfiguracionTundishControlxRango, detalle: \n" + ex.Message, ex);
            }
        }



    }
}
