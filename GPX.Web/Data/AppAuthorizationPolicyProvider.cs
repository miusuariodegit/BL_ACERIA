using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

// [GPX-DOC-v1] ================================================================================
// Proveedor de politicas de autorizacion dinamicas: genera politicas de modulo/permiso a partir de la
// configuracion de perfiles y modulos.
// ================================================================================================

namespace GPX.Web.Data {
    /// <summary>
    /// Clase AppAuthorizationPolicyProvider. Proveedor de politicas de autorizacion dinamicas: genera
    /// politicas de modulo/permiso a partir de la configuracion de perfiles y modulos.
    /// </summary>
    public class AppAuthorizationPolicyProvider(IOptions<AuthorizationOptions> options) : DefaultAuthorizationPolicyProvider(options) {
        /// <summary>
        /// Genera dinamicamente una politica de autorizacion de modulo o permiso a partir del nombre
        /// solicitado.
        /// </summary>
        public override Task<AuthorizationPolicy?> GetPolicyAsync(string policyName) {
            if(policyName.StartsWith(AppPolicies.ModulePrefix, StringComparison.OrdinalIgnoreCase)) {
                var moduleCode = policyName[AppPolicies.ModulePrefix.Length..];
                var policy = new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .RequireClaim(AppClaimTypes.Module, moduleCode)
                    .Build();
                return Task.FromResult<AuthorizationPolicy?>(policy);
            }

            if(policyName.StartsWith(AppPolicies.PermissionPrefix, StringComparison.OrdinalIgnoreCase)) {
                var permissionCode = policyName[AppPolicies.PermissionPrefix.Length..];
                var policy = new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .RequireClaim(AppClaimTypes.Permission, permissionCode)
                    .Build();
                return Task.FromResult<AuthorizationPolicy?>(policy);
            }

            return base.GetPolicyAsync(policyName);
        }
    }
}
