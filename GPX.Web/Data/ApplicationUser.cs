using Microsoft.AspNetCore.Identity;

// [GPX-DOC-v1] ================================================================================
// Extiende IdentityUser con FullName y la relacion al perfil de acceso (AppProfile).
// ================================================================================================

namespace GPX.Web.Data {
    /// <summary>
    /// Clase ApplicationUser. Extiende IdentityUser con FullName y la relacion al perfil de acceso
    /// (AppProfile).
    /// </summary>
    public class ApplicationUser : IdentityUser {
        public string FullName { get; set; } = string.Empty;
        public int? ProfileId { get; set; }
        public AppProfile? Profile { get; set; }
    }
}
