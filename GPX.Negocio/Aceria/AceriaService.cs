using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading.Tasks;

// [GPX-DOC-v1] ================================================================================
// Servicio de acceso a datos (Dapper) para consultas de negocio de Aceria contra SQL Server: necesidad
// de beam blank por tren de colada y tundish disponibles.
// ================================================================================================

namespace GPX.Negocio.Aceria
{
    /// <summary>
    /// Clase AceriaService. Servicio de acceso a datos (Dapper) para consultas de negocio de Aceria contra
    /// SQL Server: necesidad de beam blank por tren de colada y tundish disponibles.
    /// </summary>
    public  class AceriaService
    {
        private readonly string cnn;

        /// <summary>
        /// Inicializa una nueva instancia de la clase AceriaService.
        /// </summary>
        public AceriaService(IConfiguration configuration)
        {
            cnn = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("No se encontró la cadena de conexión 'DefaultConnection'.");
        }







        /// <summary>
        /// Obtiene la necesidad virtual de beam blank para un tren de colada, ejecutando el stored procedure
        /// sp_DameNecesidadVirtualBeamBlankTrenV2 filtrado por sociedad y codigo de maquina.
        /// </summary>
        public async Task<List<BeamBlankNecesidad>> DameNecesidadBeamBlankTrenV2Async(string Sociedad, string CodMaquina)
        {
            try
            {
                using (var connection = new SqlConnection(cnn))
                {
                    var parametros = new DynamicParameters();
                    parametros.Add("@Sociedad", Sociedad, DbType.String);
                    parametros.Add("@CodMaquina", CodMaquina, DbType.Int32);

                    var resultado = await connection.QueryAsync<BeamBlankNecesidad>(
                        "sp_DameNecesidadVirtualBeamBlankTrenV2",
                        parametros,
                        commandType: CommandType.StoredProcedure
                    );

                    return resultado.AsList();
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error al ejecutar sp_DameNecesidadVirtualBeamBlankTrenV2 detalle: \n" + ex.Message, ex);
            }
        }


        /// <summary>
        /// Consulta los tundish disponibles que cubren un numero de horas requeridas a partir de una fecha
        /// inicial, para un tipo de semielaborado concreto.
        /// </summary>
        public async Task<List<ListTundishDisponibles>> ConsultaTundishDisponiblesAsync(int horasRequeridas, DateTime fechaInicial, string TipoSemi)
        {
            try
            {
                using (var db = new SqlConnection(cnn))
                {
                    var resultado = await db.QueryAsync<ListTundishDisponibles>(
                        sql: "sp_CosultaTundishDisponiblesEnCalendario", param: new
                        {
                            horasRequeridas,
                            fechaInicial,
                            TipoSemi

                        }, commandType: CommandType.StoredProcedure);

                    return resultado.AsList();
                }
            }
            catch (Exception ex)
            {

                throw new Exception("Error al ejecutar sp_CosultaTundishDisponiblesEnCalendario, detalle: \n" + ex.Message, ex);
            }

        }



    }
}
