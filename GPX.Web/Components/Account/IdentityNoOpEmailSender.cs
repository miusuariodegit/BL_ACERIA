using GPX.Web.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;

// [GPX-DOC-v1] ================================================================================
// Implementacion sin efecto de IEmailSender, usada mientras no hay un proveedor de correo real
// configurado.
// ================================================================================================

namespace GPX.Web.Components.Account {
    // Remove the "else if (EmailSender is IdentityNoOpEmailSender)" block from RegisterConfirmation.razor after updating with a real implementation.
    /// <summary>
    /// Clase IdentityNoOpEmailSender. Implementacion sin efecto de IEmailSender, usada mientras no hay un
    /// proveedor de correo real configurado.
    /// </summary>
    internal sealed class IdentityNoOpEmailSender : IEmailSender<ApplicationUser> {
        private readonly IEmailSender emailSender = new NoOpEmailSender();

        /// <summary>
        /// Send Confirmation Link (operacion asincrona).
        /// </summary>
        public Task SendConfirmationLinkAsync(ApplicationUser user, string email, string confirmationLink) =>
            emailSender.SendEmailAsync(email, "Confirm your email", $"Please confirm your account by <a href='{confirmationLink}'>clicking here</a>.");

        /// <summary>
        /// Send Password Reset Link (operacion asincrona).
        /// </summary>
        public Task SendPasswordResetLinkAsync(ApplicationUser user, string email, string resetLink) =>
            emailSender.SendEmailAsync(email, "Reset your password", $"Please reset your password by <a href='{resetLink}'>clicking here</a>.");

        /// <summary>
        /// Send Password Reset Code (operacion asincrona).
        /// </summary>
        public Task SendPasswordResetCodeAsync(ApplicationUser user, string email, string resetCode) =>
            emailSender.SendEmailAsync(email, "Reset your password", $"Please reset your password using the following code: {resetCode}");
    }
}
