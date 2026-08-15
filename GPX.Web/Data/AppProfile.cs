namespace GPX.Web.Data {

// [GPX-DOC-v1] ================================================================================
// Entidad de perfil de usuario (rol funcional): nombre, descripcion, usuarios y modulos asociados.
// ================================================================================================
    /// <summary>
    /// Clase AppProfile. Entidad de perfil de usuario (rol funcional): nombre, descripcion, usuarios y
    /// modulos asociados.
    /// </summary>
    public class AppProfile {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public ICollection<ApplicationUser> Users { get; set; } = new List<ApplicationUser>();
        public ICollection<AppProfileModule> ProfileModules { get; set; } = new List<AppProfileModule>();
    }
}
