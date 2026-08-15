using System.Diagnostics;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using GPX.Web;
using GPX.Web.Data;

// [GPX-DOC-v1] ================================================================================
// Proveedor de estado de autenticacion que persiste el estado entre el render estatico y el
// interactivo de Blazor Server.
// ================================================================================================

namespace GPX.Web.Components.Account {
    // This provider persists minimal user data for interactive rendering.
    /// <summary>
    /// Clase PersistingServerAuthenticationStateProvider. Proveedor de estado de autenticacion que persiste
    /// el estado entre el render estatico y el interactivo de Blazor Server.
    /// </summary>
    internal sealed class PersistingServerAuthenticationStateProvider : ServerAuthenticationStateProvider, IDisposable {
        private readonly PersistentComponentState state;
        private readonly IdentityOptions options;

        private readonly PersistingComponentStateSubscription subscription;

        private Task<AuthenticationState>? authenticationStateTask;

        /// <summary>
        /// Inicializa una nueva instancia de la clase PersistingServerAuthenticationStateProvider.
        /// </summary>
        public PersistingServerAuthenticationStateProvider(
            PersistentComponentState persistentComponentState,
            IOptions<IdentityOptions> optionsAccessor) {
            state = persistentComponentState;
            options = optionsAccessor.Value;

            AuthenticationStateChanged += OnAuthenticationStateChanged;
            subscription = state.RegisterOnPersisting(OnPersistingAsync, RenderMode.InteractiveServer);
        }

        /// <summary>
        /// On Authentication State Changed.
        /// </summary>
        private void OnAuthenticationStateChanged(Task<AuthenticationState> task) {
            authenticationStateTask = task;
        }

        /// <summary>
        /// On Persisting (operacion asincrona).
        /// </summary>
        private async Task OnPersistingAsync() {
            if(authenticationStateTask is null) {
                throw new UnreachableException($"Authentication state not set in {nameof(OnPersistingAsync)}().");
            }

            var authenticationState = await authenticationStateTask;
            var principal = authenticationState.User;

            if(principal.Identity?.IsAuthenticated == true) {
                var userId = principal.FindFirst(options.ClaimsIdentity.UserIdClaimType)?.Value;
                var email = principal.FindFirst(options.ClaimsIdentity.EmailClaimType)?.Value;
                var name = principal.FindFirst(options.ClaimsIdentity.UserNameClaimType)?.Value;
                var role = principal.FindFirst(options.ClaimsIdentity.RoleClaimType)?.Value ?? "Guest";
                var profile = principal.FindFirst(AppClaimTypes.Profile)?.Value ?? string.Empty;

                if(userId != null && email != null && name != null && role != null) {
                    state.PersistAsJson(nameof(UserInfo), new UserInfo {
                        UserId = userId,
                        Email = email,
                        Name = name,
                        Role = role,
                        Profile = profile
                    });
                }
            }
        }

        /// <summary>
        /// Libera la suscripcion a los cambios de estado de autenticacion.
        /// </summary>
        public void Dispose() {
            subscription.Dispose();
            AuthenticationStateChanged -= OnAuthenticationStateChanged;
        }
    }
}
