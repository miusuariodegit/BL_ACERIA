using GPX.Negocio.Aceria;
using GPX.Negocio.CRUD;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

// [GPX-DOC-v1] ================================================================================
// Registro de los servicios de la capa de negocio en el contenedor de inyeccion de dependencias.
// ================================================================================================

namespace GPX.Negocio.COP
{
    /// <summary>
    /// Clase DependencyInjection. Registro de los servicios de la capa de negocio en el contenedor de
    /// inyeccion de dependencias.
    /// </summary>
    public static class DependencyInjection
    {
        /// <summary>
        /// Registra en el contenedor de dependencias los servicios de la capa de negocio (repositorio CRUD,
        /// servicios de Aceria y el estado de version de tundish seleccionada).
        /// </summary>
        public static IServiceCollection AddNegocio(this IServiceCollection services)
        {
            services.AddScoped<CrudRepository>();
            services.AddScoped<AceriaService>();
            services.AddScoped<CalendarioFusionHornoService>();
            services.AddScoped<ConfiguracionTundishService>();


            // STATE CONTAINER
            services.AddScoped<VersionTundishSeleccionadoState>();

            return services;
        }
    }
}
