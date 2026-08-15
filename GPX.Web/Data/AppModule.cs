namespace GPX.Web.Data {

// [GPX-DOC-v1] ================================================================================
// Entidad de modulo de navegacion/funcionalidad: codigo, nombre, ruta, icono, modulo padre y orden de
// visualizacion.
// ================================================================================================
    /// <summary>
    /// Clase AppModule. Entidad de modulo de navegacion/funcionalidad: codigo, nombre, ruta, icono, modulo
    /// padre y orden de visualizacion.
    /// </summary>
    public class AppModule {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Route { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string IconCssClass { get; set; } = string.Empty;
        public string ParentCode { get; set; } = string.Empty;
        public string ParentName { get; set; } = string.Empty;
        public string ParentIconCssClass { get; set; } = string.Empty;
        public int ParentDisplayOrder { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsEnabled { get; set; } = true;
        public ICollection<AppProfileModule> ProfileModules { get; set; } = new List<AppProfileModule>();
    }
}
