using System.Security.Claims;
using GPX.Web.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

// [GPX-DOC-v1] ================================================================================
// Resuelve que modulos puede ver un usuario autenticado segun su perfil, para la navegacion y el
// control de acceso.
// ================================================================================================

namespace GPX.Web.Services {
    /// <summary>
    /// Clase ModuleAccessService. Resuelve que modulos puede ver un usuario autenticado segun su perfil,
    /// para la navegacion y el control de acceso.
    /// </summary>
    public class ModuleAccessService {
        private readonly ApplicationDbContext _dbContext;
        private readonly bool _showHomePage;

        /// <summary>
        /// Inicializa una nueva instancia de la clase ModuleAccessService.
        /// </summary>
        public ModuleAccessService(ApplicationDbContext dbContext, IConfiguration configuration) {
            _dbContext = dbContext;
            _showHomePage = configuration.GetValue("Navigation:ShowHomePage", true);
        }

        public bool ShowHomePage => _showHomePage;

        /// <summary>
        /// Obtiene los modulos a los que tiene acceso el usuario autenticado.
        /// </summary>
        public async Task<IReadOnlyList<ModuleDefinition>> GetAllowedModulesAsync(ClaimsPrincipal user) {
            if(user.Identity?.IsAuthenticated != true) {
                return [];
            }

            var allowedCodes = user.FindAll(AppClaimTypes.Module)
                .Select(claim => claim.Value)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            return await _dbContext.Modules
                .AsNoTracking()
                .Where(module => module.IsEnabled && allowedCodes.Contains(module.Code))
                .OrderBy(module => module.ParentDisplayOrder)
                .ThenBy(module => module.DisplayOrder)
                .Select(module => new ModuleDefinition(
                    module.Code,
                    module.Name,
                    module.Route,
                    module.Description,
                    module.IconCssClass,
                    module.ParentCode,
                    module.ParentName,
                    module.ParentIconCssClass,
                    module.ParentDisplayOrder,
                    module.DisplayOrder))
                .ToListAsync();
        }

        /// <summary>
        /// Obtiene los modulos permitidos agrupados por modulo padre para el menu de navegacion.
        /// </summary>
        public async Task<IReadOnlyList<ModuleGroupItem>> GetAllowedModuleGroupsAsync(ClaimsPrincipal user) {
            var allowedModules = await GetAllowedModulesAsync(user);
            return allowedModules
                .GroupBy(module => new ModuleGroupDefinition(
                    module.ParentCode,
                    module.ParentName,
                    module.ParentIconCssClass,
                    module.ParentDisplayOrder))
                .OrderBy(group => group.Key.DisplayOrder)
                .Select(group => new ModuleGroupItem(
                    group.Key,
                    group.OrderBy(module => module.DisplayOrder).ToList()))
                .ToList();
        }

        /// <summary>
        /// Resuelve el nombre visible de un modulo a partir de su ruta.
        /// </summary>
        public string ResolveModuleName(string route, IReadOnlyList<ModuleDefinition> visibleModules) {
            var normalizedRoute = route.Trim('/');
            return visibleModules.FirstOrDefault(module =>
                    string.Equals(module.Route.Trim('/'), normalizedRoute, StringComparison.OrdinalIgnoreCase))
                ?.Name
                ?? normalizedRoute;
        }

        /// <summary>
        /// Obtiene el nombre del perfil del usuario autenticado.
        /// </summary>
        public string GetProfileName(ClaimsPrincipal user) =>
            user.FindFirst(AppClaimTypes.Profile)?.Value ?? "Sin perfil";

        /// <summary>
        /// Obtiene el nombre para mostrar del usuario autenticado.
        /// </summary>
        public string GetDisplayName(ClaimsPrincipal user) =>
            user.FindFirst(ClaimTypes.Name)?.Value
            ?? user.FindFirst(ClaimTypes.Email)?.Value
            ?? "Usuario";
    }

    /// <summary>
    /// Registro ModuleGroupItem. Resuelve que modulos puede ver un usuario autenticado segun su perfil,
    /// para la navegacion y el control de acceso.
    /// </summary>
    public sealed record ModuleGroupItem(
        ModuleGroupDefinition Group,
        IReadOnlyList<ModuleDefinition> Modules);
}
