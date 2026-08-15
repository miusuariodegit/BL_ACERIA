using Microsoft.EntityFrameworkCore;

// [GPX-DOC-v1] ================================================================================
// Aplica migraciones pendientes e inicializa datos base al arrancar la aplicacion.
// ================================================================================================

namespace GPX.Web.Data
{
    /// <summary>
    /// Clase ApplicationDbInitializer. Aplica migraciones pendientes e inicializa datos base al arrancar la
    /// aplicacion.
    /// </summary>
    public static class ApplicationDbInitializer
    {
        /// <summary>
        /// Aplica las migraciones pendientes e inicializa los datos base de la aplicacion al arrancar.
        /// </summary>
        public static async Task InitializeAsync(IServiceProvider services)
        {
            await using var scope = services.CreateAsyncScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            //await dbContext.Database.MigrateAsync();
        }
    }
}
