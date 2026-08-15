namespace GPX.Web.Data {

// [GPX-DOC-v1] ================================================================================
// Tabla puente entre AppProfile y AppModule que define que modulos ve cada perfil.
// ================================================================================================
    /// <summary>
    /// Clase AppProfileModule. Tabla puente entre AppProfile y AppModule que define que modulos ve cada
    /// perfil.
    /// </summary>
    public class AppProfileModule {
        public int ProfileId { get; set; }
        public AppProfile Profile { get; set; } = default!;
        public int ModuleId { get; set; }
        public AppModule Module { get; set; } = default!;
    }
}
