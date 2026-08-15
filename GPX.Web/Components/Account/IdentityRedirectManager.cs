using Microsoft.AspNetCore.Components;

// [GPX-DOC-v1] ================================================================================
// Helper de redirecciones tras operaciones de Identity, con o sin mensaje de estado.
// ================================================================================================

namespace GPX.Web.Components.Account {
    /// <summary>
    /// Clase IdentityRedirectManager. Helper de redirecciones tras operaciones de Identity, con o sin
    /// mensaje de estado.
    /// </summary>
    internal sealed class IdentityRedirectManager(
        NavigationManager navigationManager,
        IHttpContextAccessor httpContextAccessor) {
        public const string StatusCookieName = "Identity.StatusMessage";

        private static readonly CookieBuilder StatusCookieBuilder = new() {
            SameSite = SameSiteMode.Strict,
            HttpOnly = true,
            IsEssential = true,
            MaxAge = TimeSpan.FromSeconds(5),
        };

        /// <summary>
        /// Redirect To.
        /// </summary>
        public void RedirectTo(string? uri) {
            uri ??= "";

            // Prevent open redirects.
            if(!Uri.IsWellFormedUriString(uri, UriKind.Relative)) {
                uri = navigationManager.ToBaseRelativePath(uri);
            }

            var absoluteUri = navigationManager.ToAbsoluteUri(uri).ToString();

            var httpContext = httpContextAccessor.HttpContext;
            if(httpContext is not null && !httpContext.Response.HasStarted) {
                httpContext.Response.Redirect(absoluteUri);
                return;
            }

            navigationManager.NavigateTo(uri);
        }

        /// <summary>
        /// Redirect To.
        /// </summary>
        public void RedirectTo(string uri, Dictionary<string, object?> queryParameters) {
            var uriWithoutQuery = navigationManager.ToAbsoluteUri(uri).GetLeftPart(UriPartial.Path);
            var newUri = navigationManager.GetUriWithQueryParameters(uriWithoutQuery, queryParameters);
            RedirectTo(newUri);
        }

        /// <summary>
        /// Redirect To With Status.
        /// </summary>
        public void RedirectToWithStatus(string uri, string message, HttpContext context) {
            context.Response.Cookies.Append(StatusCookieName, message, StatusCookieBuilder.Build(context));
            RedirectTo(uri);
        }

        private string CurrentPath => navigationManager.ToAbsoluteUri(navigationManager.Uri).GetLeftPart(UriPartial.Path);

        /// <summary>
        /// Redirect To Current Page.
        /// </summary>
        public void RedirectToCurrentPage() => RedirectTo(CurrentPath);

        /// <summary>
        /// Redirect To Current Page With Status.
        /// </summary>
        public void RedirectToCurrentPageWithStatus(string message, HttpContext context)
            => RedirectToWithStatus(CurrentPath, message, context);
    }
}
