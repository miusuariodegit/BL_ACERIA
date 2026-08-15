using GPX.Web.Services;
using DevExpress.Blazor;
using Microsoft.AspNetCore.Components;

// [GPX-DOC-v1] ================================================================================
// Registro de los servicios propios de GPX.Web (branding, temas, accesos por modulo) en el contenedor
// de dependencias.
// ================================================================================================

namespace GPX.Web.Utils {
    /// <summary>
    /// Clase ServiceExtensions. Registro de los servicios propios de GPX.Web (branding, temas, accesos por
    /// modulo) en el contenedor de dependencias.
    /// </summary>
    public static class ServiceExtensions {
        /// <summary>
        /// Registra en el contenedor de dependencias los servicios propios de la aplicacion web (branding,
        /// temas, accesos por modulo, etc.).
        /// </summary>
        public static void AddAppServices(this IServiceCollection services) {
            services.AddDevExpressBlazor();
            services.AddScoped<BrandingService>();
            services.AddScoped<GestionManagerService>();
            services.AddScoped<ModuleLoader>();
            services.AddScoped<ModuleAccessService>();
            services.AddScoped<ThemeManager>();
            services.AddScoped<SizeModeManager>();
            services.AddScoped(sp => new CascadingValueSource<SizeMode>("ParentSizeMode", SizeMode.Medium, false));
            services.AddCascadingValue(sp => sp.GetRequiredService<CascadingValueSource<SizeMode>>());
        }
    }
}
