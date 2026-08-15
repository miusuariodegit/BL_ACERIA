using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

// [GPX-DOC-v1] ================================================================================
// Genera los claims del usuario autenticado, incluyendo utilidades para anadir o eliminar claims de un
// ClaimsIdentity.
// ================================================================================================

namespace GPX.Web.Data {
    /// <summary>
    /// Clase ApplicationUserClaimsPrincipalFactory. Genera los claims del usuario autenticado, incluyendo
    /// utilidades para anadir o eliminar claims de un ClaimsIdentity.
    /// </summary>
    public class ApplicationUserClaimsPrincipalFactory(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        IOptions<IdentityOptions> optionsAccessor,
        ApplicationDbContext dbContext)
        : UserClaimsPrincipalFactory<ApplicationUser, IdentityRole>(userManager, roleManager, optionsAccessor) {

        /// <summary>
        /// Genera el ClaimsPrincipal del usuario autenticado anadiendo los claims propios de la aplicacion.
        /// </summary>
        public override async Task<ClaimsPrincipal> CreateAsync(ApplicationUser user) {
            var principal = await base.CreateAsync(user);
            var identity = (ClaimsIdentity)principal.Identity!;

            identity.RemoveClaimIfExists(Options.ClaimsIdentity.UserNameClaimType);
            identity.RemoveClaimIfExists(AppClaimTypes.Profile);
            identity.RemoveClaims(AppClaimTypes.Module);

            var fullName = string.IsNullOrWhiteSpace(user.FullName) ? user.Email ?? user.UserName ?? "Usuario" : user.FullName;
            identity.AddClaim(new Claim(Options.ClaimsIdentity.UserNameClaimType, fullName));

            var dbUser = await dbContext.Users
                .AsNoTracking()
                .Include(record => record.Profile)
                .ThenInclude(profile => profile!.ProfileModules)
                .ThenInclude(link => link.Module)
                .FirstOrDefaultAsync(record => record.Id == user.Id);

            var profileName = dbUser?.Profile?.Name;
            var modules = dbUser?.Profile?.ProfileModules
                .Where(link => link.Module.IsEnabled)
                .Select(link => link.Module.Code)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList()
                ?? [];

            if(!string.IsNullOrWhiteSpace(profileName)) {
                identity.AddClaim(new Claim(AppClaimTypes.Profile, profileName));
            }

            foreach(var moduleCode in modules) {
                identity.AddClaim(new Claim(AppClaimTypes.Module, moduleCode));
            }

            return principal;
        }
    }

    /// <summary>
    /// Clase ClaimsIdentityExtensions. Genera los claims del usuario autenticado, incluyendo utilidades
    /// para anadir o eliminar claims de un ClaimsIdentity.
    /// </summary>
    internal static class ClaimsIdentityExtensions {
        /// <summary>
        /// Remove Claim If Exists.
        /// </summary>
        public static void RemoveClaimIfExists(this ClaimsIdentity identity, string claimType) {
            var existingClaims = identity.FindAll(claimType).ToList();
            foreach(var claim in existingClaims) {
                identity.RemoveClaim(claim);
            }
        }

        /// <summary>
        /// Remove Claims.
        /// </summary>
        public static void RemoveClaims(this ClaimsIdentity identity, string claimType) =>
            identity.RemoveClaimIfExists(claimType);
    }
}
