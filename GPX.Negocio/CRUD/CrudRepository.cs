using Dapper;
using GPX.Negocio.ORM;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Protocols;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading.Tasks;

// [GPX-DOC-v1] ================================================================================
// Repositorio generico con operaciones de alta, consulta, actualizacion y eliminacion (Dapper) para
// las entidades principales del dominio de Aceria.
// ================================================================================================

namespace GPX.Negocio.CRUD
{
    /// <summary>
    /// Clase CrudRepository. Repositorio generico con operaciones de alta, consulta, actualizacion y
    /// eliminacion (Dapper) para las entidades principales del dominio de Aceria.
    /// </summary>
    public  class CrudRepository
    {

        private readonly string cnn;

        /// <summary>
        /// Inicializa una nueva instancia de la clase CrudRepository.
        /// </summary>
        public CrudRepository(IConfiguration configuration)
        {
            cnn = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("No se encontró la cadena de conexión 'DefaultConnection'.");
        }

        private static void CopiarPropiedades<T>(T origen, T destino)
        {       
            if (origen == null || destino == null)
            {
                return;
            }

            var propiedades = typeof(T).GetProperties();
            foreach (var propiedad in propiedades)
            {
                if (propiedad.CanRead && propiedad.CanWrite)
                {
                    propiedad.SetValue(destino, propiedad.GetValue(origen));
                }
            }
        }



        #region GestionStockSemi

        /// <summary>
        /// Inserta un nuevo registro de gestion stock semi.
        /// </summary>
        public async Task<Boolean> AltaGestionStockSemiAsync(GestionStockSemi values)
        {
            try
            {
                using (var db = new SqlConnection(cnn))
                {
                    await db.ExecuteAsync(sql: "sp_CRUD_GestionStockSemi_Insert", param: new
                    {
                        //values.geId,
                        values.geFechaOperacion,
                        values.geTipoOperacion,
                        values.geIdFabGPB,
                        values.geCalidadSemi,
                        values.geTnBrutas,
                        values.geTnStock,
                        values.geTnAsignadas,
                        values.geTnAsignadasTotal,
                        values.geTnLibres,
                        values.geFamilia,
                        values.geLong,
                        values.gelongSemi,
                        values.gePesoSemi,
                        values.geTipoSemi,
                        values.geNombreUsuario,
                        values.geUnAsignadas,
                        values.geUnBrutas,
                        values.geUnStock,
                        values.geUnAsignadasTotal,
                        values.geUnLibres


                    }, commandType: CommandType.StoredProcedure);
                }

                return true;

            }
            catch (Exception ex)
            {

                throw new Exception("Error al ejecutar sp_CRUD_GestionStockSemi_Insert, detalle: \n" + ex.Message, ex);
            }
        }
        /// <summary>
        /// Actualiza un registro existente de gestion stock semi.
        /// </summary>
        public async Task<Boolean> ActualizarGestionStockSemiAsync(GestionStockSemi values)
        {
            try
            {
                using (var db = new SqlConnection(cnn))
                {
                    await db.ExecuteAsync(sql: "sp_CRUD_GestionStockSemi_Update", param: new
                    {
                        values.geId,
                        values.geFechaOperacion,
                        values.geTipoOperacion,
                        values.geIdFabGPB,
                        values.geCalidadSemi,
                        values.geTnBrutas,
                        values.geTnStock,
                        values.geTnAsignadas,
                        values.geTnAsignadasTotal,
                        values.geTnLibres,
                        values.geFamilia,
                        values.geLong,
                        values.gelongSemi,
                        values.gePesoSemi,
                        values.geTipoSemi,
                        values.geNombreUsuario,
                        values.geUnAsignadas,
                        values.geUnBrutas,
                        values.geUnStock,
                        values.geUnAsignadasTotal,
                        values.geUnLibres



                    }, commandType: CommandType.StoredProcedure);
                }

                return true;
            }
            catch (Exception ex)
            {
                throw new Exception("Error al ejecutar sp_CRUD_GestionStockSemi_Update, detalle: \n" + ex.Message, ex);
            }
        }

        /// <summary>
        /// Elimina un registro de gestion stock semi.
        /// </summary>
        public async Task<Boolean> EliminarGestionStockSemiAsync(GestionStockSemi values)
        {
            try
            {
                using (var db = new SqlConnection(cnn))
                {
                    await db.ExecuteAsync(sql: "sp_CRUD_GestionStockSemi_Delete", param: new
                    {
                        values.geId

                    }, commandType: CommandType.StoredProcedure);
                }
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception("Error al ejecutar sp_CRUD_GestionStockSemi_Delete, detalle: \n" + ex.Message, ex);
            }
        }



        #endregion

        #region OrdenCalidadPlanificacion 

        /// <summary>
        /// Consulta los registros de orden calidad planificacion.
        /// </summary>
        public async Task<List<OrdenCalidadPlanificacion>> ConsultaOrdenCalidadPlanificacionAsync()
        {
            try
            {
                List<OrdenCalidadPlanificacion> resultado = new List<OrdenCalidadPlanificacion>();

                using (var db = new SqlConnection(cnn))
                {
                    resultado = (await db.QueryAsync<OrdenCalidadPlanificacion>(sql: "sp_DameOrdenCalidadesBalboa")).ToList();
                }
                return resultado;
            }
            catch (Exception ex)
            {

                throw new Exception("Error al ejecutar sp_DameOrdenCalidadesBalboa, detalle: \n" + ex.Message, ex);
            }
        }
        #endregion

        #region CalendarioFusionHorno

        /// <summary>
        /// Consulta los registros de calendario fusion horno.
        /// </summary>
        public async Task<List<CalendarioFusionHorno>> ConsultaCalendarioFusionHornoAsync()
        {
            try
            {
                List<CalendarioFusionHorno> resultado = new List<CalendarioFusionHorno>();

                using (var db = new SqlConnection(cnn))
                {
                    resultado = (await db.QueryAsync<CalendarioFusionHorno>(sql: "[sp_CRUD_CalendarioFusionHorno_Select]")).ToList();
                }
                return resultado;
            }
            catch (Exception ex)
            {

                throw new Exception("Error al ejecutar [sp_CRUD_CalendarioFusionHorno_Select], detalle: \n" + ex.Message, ex);
            }
        }

        /// <summary>
        /// Inserta un nuevo registro de calendario fusion horno.
        /// </summary>
        public async Task<Boolean> AltaCalendarioFusionHornoAsync(CalendarioFusionHorno values)
        {
            try
            {
                using (var db = new SqlConnection(cnn))
                {
                    await db.ExecuteAsync(sql: "sp_CRUD_CalendarioFusionHorno_Insert", param: new
                    {
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

                throw new Exception("Error al ejecutar sp_CRUD_CalendarioFusionHorno_Insert, detalle: \n" + ex.Message, ex);
            }
        }
        /// <summary>
        /// Actualiza un registro existente de calendario fusion horno.
        /// </summary>
        public async Task<Boolean> ActualizarCalendarioFusionHornoAsync(CalendarioFusionHorno values)
        {
            try
            {
                using (var db = new SqlConnection(cnn))
                {
                    await db.ExecuteAsync(sql: "sp_CRUD_CalendarioFusionHorno_Update", param: new
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
                throw new Exception("Error al ejecutar sp_CRUD_CalendarioFusionHorno_Update, detalle: \n" + ex.Message, ex);
            }
        }

        /// <summary>
        /// Elimina un registro de calendario fusion horno.
        /// </summary>
        public async Task<Boolean> EliminarCalendarioFusionHornoAsync(CalendarioFusionHorno values)
        {
            try
            {
                using (var db = new SqlConnection(cnn))
                {
                    await db.ExecuteAsync(sql: "sp_CRUD_CalendarioFusionHorno_Delete", param: new
                    {
                        values.cafId

                    }, commandType: CommandType.StoredProcedure);
                }
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception("Error al ejecutar sp_CRUD_CalendarioFusionHorno_Delete, detalle: \n" + ex.Message, ex);
            }
        }



        #endregion

        #region TundishStandard
        //genera las funciones para el CRUD de la tabla TundishStandard


        /// <summary>
        /// Consulta los registros de tundish standard.
        /// </summary>
        public async Task<List<TundishStandard>> ConsultaTundishStandardAsync()
        {
            try
            {
                List<TundishStandard> resultado = new List<TundishStandard>();

                using (var db = new SqlConnection(cnn))
                {
                    resultado = (await db.QueryAsync<TundishStandard>(sql: "sp_CRUD_TundishStandard_Select")).ToList();
                }
                return resultado;
            }
            catch (Exception ex)
            {

                throw new Exception("Error al ejecutar sp_CRUD_TundishStandard_Select, detalle: \n" + ex.Message, ex);
            }
        }

        /// <summary>
        /// Inserta un nuevo registro de tundish standard.
        /// </summary>
        public async Task<Boolean> AltaTundishStandardAsync(TundishStandard values)
        {
            try
            {
                using (var db = new SqlConnection(cnn))
                {
                    await db.ExecuteAsync(sql: "sp_CRUD_TundishStandard_Insert", param: new
                    {
                        values.tsSociedad,
                        values.tsCierre1,
                        values.tsCierre2,
                        values.tsCierre3,
                        values.tsCierre4,
                        values.tsCierre5,
                        values.tsCierre6,
                        values.tsActivo,
                        values.tsPrioridad,
                        values.tstotalBB1,
                        values.tstotalBB2,
                        values.tstotalBB3,


                    }, commandType: CommandType.StoredProcedure);
                }

                return true;

            }
            catch (Exception ex)
            {

                throw new Exception("Error al ejecutar sp_CRUD_TundishStandard_Insert, detalle: \n" + ex.Message, ex);
            }
        }
        /// <summary>
        /// Actualiza un registro existente de tundish standard.
        /// </summary>
        public async Task<Boolean> ActualizarTundishStandardAsync(TundishStandard values)
        {
            try
            {
                using (var db = new SqlConnection(cnn))
                {
                    await db.ExecuteAsync(sql: "sp_CRUD_TundishStandard_Update", param: new
                    {
                        values.tsId,
                        values.tsSociedad,
                        values.tsCierre1,
                        values.tsCierre2,
                        values.tsCierre3,
                        values.tsCierre4,
                        values.tsCierre5,
                        values.tsCierre6,
                        values.tsActivo,
                        values.tsPrioridad,
                        values.tstotalBB1,
                        values.tstotalBB2,
                        values.tstotalBB3,


                    }, commandType: CommandType.StoredProcedure);
                }

                return true;
            }
            catch (Exception ex)
            {
                throw new Exception("Error al ejecutar sp_CRUD_TundishStandard_Update, detalle: \n" + ex.Message, ex);
            }
        }
        /// <summary>
        /// Elimina un registro de calendario fusion horno.
        /// </summary>
        public async Task<Boolean> EliminarCalendarioFusionHornoAsync(TundishStandard values)
        {
            try
            {
                using (var db = new SqlConnection(cnn))
                {
                    await db.ExecuteAsync(sql: "sp_CRUD_TundishStandard_Delete", param: new
                    {
                        values.tsId

                    }, commandType: CommandType.StoredProcedure);
                }
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception("Error al ejecutar sp_CRUD_TundishStandard_Delete, detalle: \n" + ex.Message, ex);
            }
        }




        #endregion

        #region Tundish

        /// <summary>
        /// Inserta un nuevo registro de tundish.
        /// </summary>
        public async Task<Boolean> AltaTundishAsync(Tundish values)
        {
            try
            {
                Tundish nuevoObjeto = new Tundish();
                using (var db = new SqlConnection(cnn))
                {
                    nuevoObjeto = await db.QuerySingleAsync<Tundish>(sql: "sp_CRUD_Tundish_Insert", param: new
                    {

                        values.tuSociedad,
                        values.tuNecesidadBB1,
                        values.tuNecesidadBB2,
                        values.tuNecesidadBB3,
                        values.@tuActivo,
                        values.@tuTipoStandard,
                        values.@tuCierre1Real,
                        values.@tuCierre2Real,
                        values.@tuCierre4Real,
                        values.@tuCierre5Real,
                        values.@tuCierre6Real


                    }, commandType: CommandType.StoredProcedure);
                }
                CopiarPropiedades(nuevoObjeto, values);
                return true;

            }
            catch (Exception ex)
            {

                throw new Exception("Error al ejecutar sp_CRUD_Tundish_Insert, detalle: \n" + ex.Message, ex);
            }
        }
        #endregion

        #region ConfiguracionAceria   
        /// <summary>
        /// Consulta los registros de configuracion aceria.
        /// </summary>
        public async Task<List<ConfiguracionAceria>> ConsultaConfiguracionAceriaAsync()
        {
            try
            {
                List<ConfiguracionAceria> resultado = new List<ConfiguracionAceria>();

                using (var db = new SqlConnection(cnn))
                {
                    resultado = (await db.QueryAsync<ConfiguracionAceria>(sql: "sp_CRUD_ConfiguracionAceria_Select")).ToList();
                }
                return resultado;
            }
            catch (Exception ex)
            {

                throw new Exception("Error al ejecutar sp_CRUD_ConfiguracionAceria_Select, detalle: \n" + ex.Message, ex);
            }
        }

        /// <summary>
        /// Actualiza un registro existente de configuracion aceria.
        /// </summary>
        public async Task<Boolean> ActualizarConfiguracionAceriaAsync(ConfiguracionAceria values)
        {
            try
            {
                using (var db = new SqlConnection(cnn))
                {
                    await db.ExecuteAsync(sql: "sp_CRUD_ConfiguracionAceria_Update", param: new
                    {
                        values.caId,
                        values.caCodSociedad,
                        values.caCodMaquina,
                        values.caToneladasXCuchara,
                        values.caTiempoMinColada,
                        values.caTiempoMaxColada,
                        values.caTiempoLF,
                        values.caMaximoPerfiles,
                        values.caMaxPerfilesXTipo,
                        values.caVidaUtilTundish,
                        values.caPesoLinealBB1,
                        values.caPesoLinealBB2,
                        values.caPesoLinealBB3,
                        values.caVelocidadMaximaBB1,
                        values.caVelocidadMaximaBB2,
                        values.caVelocidadMaximaBB3,
                        values.caMaximoFabricacionesEstandar,
                        values.caColadasCalidadEstandarMinimas,
                        values.caColadasCalidadEstandarMaximas,
                        values.caMinutosCambioTundish


                    }, commandType: CommandType.StoredProcedure);
                }





                return true;
            }
            catch (Exception ex)
            {
                throw new Exception("Error al ejecutar sp_CRUD_ConfiguracionAceria_Update, detalle: \n" + ex.Message, ex);
            }
        }
        #endregion

        #region ControlColadasCargadas

        /// <summary>
        /// Inserta un nuevo registro de control coladas cargadas.
        /// </summary>
        public async Task<Boolean> AltaControlColadasCargadasAsync(ControlColadasCargadas values)
        {
            try
            {
                ControlColadasCargadas nuevoObjeto = new ControlColadasCargadas();
                using (var db = new SqlConnection(cnn))
                {
                    nuevoObjeto = await db.QuerySingleAsync<ControlColadasCargadas>(sql: "sp_CRUD_ControlColadasCargadas_Insert", param: new
                    {
                        values.IdFabGpb,
                        values.IdColada,
                        values.CalidadSemi,
                        values.TipoSemi,
                        values.LongitudSemi,
                        values.UnCargadas,
                        values.FechaCarga,
                        values.UsrCargo,
                        values.IdTemporalFam,
                        values.OndenFab

                    }, commandType: CommandType.StoredProcedure);
                }
                // values = nuevoObjeto;
                return true;

            }
            catch (Exception ex)
            {

                throw new Exception("Error al ejecutar sp_CRUD_ControlColadasCargadas_Insert, detalle: \n" + ex.Message, ex);
            }
        }


        /// <summary>
        /// Elimina un registro de control coladas cargadas.
        /// </summary>
        public async Task<Boolean> EliminarControlColadasCargadasAsync(int IdRegistro)
        {
            try
            {
                using (var db = new SqlConnection(cnn))
                {
                    await db.ExecuteAsync(sql: "sp_CRUD_ControlColadasCargadas_Delete", param: new
                    {
                        IdRegistro

                    }, commandType: CommandType.StoredProcedure);
                }
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception("Error al ejecutar sp_CRUD_ControlColadasCargadas_Delete, detalle: \n" + ex.Message, ex);
            }
        }



        #endregion

        #region ConfiguracionTundishControl

        /// <summary>
        /// Consulta los registros de configuracion tundish control.
        /// </summary>
        public async Task<List<ConfiguracionTundishControl>> ConsultaConfiguracionTundishControlAsync()
        {
            try
            {
                List<ConfiguracionTundishControl> resultado = new List<ConfiguracionTundishControl>();
                using (var db = new SqlConnection(cnn))
                {
                    resultado = (await db.QueryAsync<ConfiguracionTundishControl>(sql: "sp_CRUD_ConfiguracionTundishControl_Select")).ToList();
                }
                return resultado;
            }
            catch (Exception ex)
            {
                throw new Exception("Error al ejecutar sp_CRUD_ConfiguracionTundishControl_Select, detalle: \n" + ex.Message, ex);
            }
        }

        /// <summary>
        /// Inserta un nuevo registro de configuracion tundish control.
        /// </summary>
        public async Task<Boolean> AltaConfiguracionTundishControlAsync(ConfiguracionTundishControl values)
        {
            try
            {
                ConfiguracionTundishControl nuevoRegistro = new ConfiguracionTundishControl();
                using (var db = new SqlConnection(cnn))
                {
                    nuevoRegistro = await db.QuerySingleAsync<ConfiguracionTundishControl>(sql: "sp_CRUD_ConfiguracionTundishControl_Insert", param: new
                    {
                        values.IdVersion,
                        values.UsuarioAutor,
                        values.FechaCreacionVersion,
                        values.FechaUltimaModificacion,
                        values.NecesidadfechaInicio,
                        values.NecesidadfechaFin,
                        values.NecesidadBB1,
                        values.NecesidadBB2,
                        values.NecesidadBB3,
                        values.NecesidadTotal,
                        values.EstandarSeleccionado,
                        values.ListaConfiguracionAceria,
                        values.ListaNecesidadesBeamBlanks,
                        values.ListaNecesidadesBeamBlanksAgrupago,
                        values.ListaTundishReales,
                        values.ListaNecesidadDetalleBB,
                        values.ListaPropuestaDistribucionColadas,
                        values.CantidadTundish,
                        values.NumColadasReales,
                        values.NumMinutosNeceReales,
                        values.Estatus,
                        values.NecesidadBB1Real,
                        values.NecesidadBB2Real,
                        values.NecesidadBB3Real,
                        values.NecesidadTotalReal
                    }, commandType: CommandType.StoredProcedure);
                }
                CopiarPropiedades(nuevoRegistro, values);
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception("Error al ejecutar sp_CRUD_ConfiguracionTundishControl_Insert, detalle: \n" + ex.Message, ex);
            }
        }

        /// <summary>
        /// Actualiza un registro existente de configuracion tundish control.
        /// </summary>
        public async Task<Boolean> ActualizarConfiguracionTundishControlAsync(ConfiguracionTundishControl values)
        {
            try
            {
                using (var db = new SqlConnection(cnn))
                {
                    await db.ExecuteAsync(sql: "sp_CRUD_ConfiguracionTundishControl_Update", param: new
                    {
                        values.IdVersion,
                        values.UsuarioAutor,
                        values.FechaCreacionVersion,
                        values.FechaUltimaModificacion,
                        values.NecesidadfechaInicio,
                        values.NecesidadfechaFin,
                        values.NecesidadBB1,
                        values.NecesidadBB2,
                        values.NecesidadBB3,
                        values.NecesidadTotal,
                        values.EstandarSeleccionado,
                        values.ListaConfiguracionAceria,
                        values.ListaNecesidadesBeamBlanks,
                        values.ListaNecesidadesBeamBlanksAgrupago,
                        values.ListaTundishReales,
                        values.ListaNecesidadDetalleBB,
                        values.ListaPropuestaDistribucionColadas,
                        values.CantidadTundish,
                        values.NumColadasReales,
                        values.NumMinutosNeceReales,
                        values.Estatus,
                        values.NecesidadBB1Real,
                        values.NecesidadBB2Real,
                        values.NecesidadBB3Real,
                        values.NecesidadTotalReal
                    }, commandType: CommandType.StoredProcedure);
                }
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception("Error al ejecutar sp_CRUD_ConfiguracionTundishControl_Update, detalle: \n" + ex.Message, ex);
            }
        }

        /// <summary>
        /// Elimina un registro de configuracion tundish control.
        /// </summary>
        public async Task<Boolean> EliminarConfiguracionTundishControlAsync(long IdVersion)
        {
            try
            {
                using (var db = new SqlConnection(cnn))
                {
                    await db.ExecuteAsync(sql: "sp_CRUD_ConfiguracionTundishControl_Delete", param: new
                    {
                        IdVersion
                    }, commandType: CommandType.StoredProcedure);
                }
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception("Error al ejecutar sp_CRUD_ConfiguracionTundishControl_Delete, detalle: \n" + ex.Message, ex);
            }
        }

        #endregion

        #region CentrosXsociedad

        /// <summary>
        /// Consulta los registros de centros xsociedad.
        /// </summary>
        public async Task<List<CentrosXsociedad>> ConsultaCentrosXsociedadAsync()
        {
            try
            {
                List<CentrosXsociedad> resultado = new List<CentrosXsociedad>();
                using (var db = new SqlConnection(cnn))
                {
                    resultado = (await db.QueryAsync<CentrosXsociedad>(sql: "sp_CRUD_CentrosXsociedad_Select")).ToList();
                }
                return resultado;
            }
            catch (Exception ex)
            {
                throw new Exception("Error al ejecutar sp_CRUD_CentrosXsociedad_Select, detalle: \n" + ex.Message, ex);
            }
        }

        /// <summary>
        /// Inserta un nuevo registro de centros xsociedad.
        /// </summary>
        public async Task<Boolean> AltaCentrosXsociedadAsync(CentrosXsociedad values)
        {
            try
            {
                CentrosXsociedad nuevoRegistro = new CentrosXsociedad();
                using (var db = new SqlConnection(cnn))
                {
                    nuevoRegistro = await db.QuerySingleAsync<CentrosXsociedad>(sql: "sp_CRUD_CentrosXsociedad_Insert", param: new
                    {
                        values.csID,
                        values.csCodSociedad,
                        values.csCodCentro,
                        values.csCodSAP,
                        values.csCodGESAC,
                        values.csNombre,
                        values.csActivo,
                        values.csRendimiento,
                        values.csMerma
                    }, commandType: CommandType.StoredProcedure);
                }
                CopiarPropiedades(nuevoRegistro, values);
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception("Error al ejecutar sp_CRUD_CentrosXsociedad_Insert, detalle: \n" + ex.Message, ex);
            }
        }

        /// <summary>
        /// Actualiza un registro existente de centros xsociedad.
        /// </summary>
        public async Task<Boolean> ActualizarCentrosXsociedadAsync(CentrosXsociedad values)
        {
            try
            {
                using (var db = new SqlConnection(cnn))
                {
                    await db.ExecuteAsync(sql: "sp_CRUD_CentrosXsociedad_Update", param: new
                    {
                        values.csID,
                        values.csCodSociedad,
                        values.csCodCentro,
                        values.csCodSAP,
                        values.csCodGESAC,
                        values.csNombre,
                        values.csActivo,
                        values.csRendimiento,
                        values.csMerma
                    }, commandType: CommandType.StoredProcedure);
                }
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception("Error al ejecutar sp_CRUD_CentrosXsociedad_Update, detalle: \n" + ex.Message, ex);
            }
        }

        /// <summary>
        /// Elimina un registro de centros xsociedad.
        /// </summary>
        public async Task<Boolean> EliminarCentrosXsociedadAsync(int csID)
        {
            try
            {
                using (var db = new SqlConnection(cnn))
                {
                    await db.ExecuteAsync(sql: "sp_CRUD_CentrosXsociedad_Delete", param: new
                    {
                        csID
                    }, commandType: CommandType.StoredProcedure);
                }
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception("Error al ejecutar sp_CRUD_CentrosXsociedad_Delete, detalle: \n" + ex.Message, ex);
            }
        }

        #endregion


    }
}
