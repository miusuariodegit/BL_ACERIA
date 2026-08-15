using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc.Routing;

// [GPX-DOC-v1] ================================================================================
// Extiende CookieAuthenticationEvents para redirigir correctamente al login cuando expira la sesion en
// Blazor Server.
// ================================================================================================

namespace GPX.Web.Components.Account {
    /// <summary>
    /// Clase CookieEvents. Extiende CookieAuthenticationEvents para redirigir correctamente al login cuando
    /// expira la sesion en Blazor Server.
    /// </summary>
    public class CookieEvents : CookieAuthenticationEvents {
        /// <summary>
        /// Redirige a la pagina de login cuando la cookie de autenticacion expira o es invalida.
        /// </summary>
        public override Task RedirectToLogin(RedirectContext<CookieAuthenticationOptions> context) {
            var path = context.Request.Path;
            var redirectUri = UriHelper.BuildRelative(
                context.Request.PathBase,
                "/Account/Login",
                QueryString.Create("ReturnUrl", path.HasValue ? path.Value.TrimStart('/') : string.Empty)
            );
            context.RedirectUri = redirectUri;
            return base.RedirectToLogin(context);
        }
    }
}
