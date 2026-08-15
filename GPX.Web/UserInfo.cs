namespace GPX.Web {

// [GPX-DOC-v1] ================================================================================
// Modelo con el perfil del usuario autenticado usado en el estado de autenticacion persistente.
// ================================================================================================
    /// <summary>
    /// Clase UserInfo. Modelo con el perfil del usuario autenticado usado en el estado de autenticacion
    /// persistente.
    /// </summary>
    public class UserInfo {
        public required string UserId { get; set; }
        public required string Email { get; set; }
        public required string Name { get; set; }
        public required string Role { get; set; }
        public string Profile { get; set; } = string.Empty;
    }
}
