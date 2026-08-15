using GPX.Web.Data;
using Microsoft.AspNetCore.Identity;

// [GPX-DOC-v1] ================================================================================
// Obtiene el usuario autenticado actual o lanza una excepcion si no existe.
// ================================================================================================

namespace GPX.Web.Components.Account {
    /// <summary>
    /// Clase IdentityUserAccessor. Obtiene el usuario autenticado actual o lanza una excepcion si no
    /// existe.
    /// </summary>
    internal sealed class IdentityUserAccessor(UserManager<ApplicationUser> userManager, IdentityRedirectManager redirectManager) {
        /// <summary>
        /// Obtiene el usuario autenticado actual o lanza una excepcion si no existe.
        /// </summary>
        public async Task<ApplicationUser> GetRequiredUserAsync(HttpContext context) {
            var user = await userManager.GetUserAsync(context.User);

            if(user is null) {
                redirectManager.RedirectToWithStatus("Account/InvalidUser", $"Error: Unable to load user with ID '{userManager.GetUserId(context.User)}'.", context);
            }

            return user;
        }
    }
}
