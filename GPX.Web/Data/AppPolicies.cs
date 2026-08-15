namespace GPX.Web.Data {

// [GPX-DOC-v1] ================================================================================
// Helpers estaticos para construir nombres de politica de autorizacion por modulo o por permiso.
// ================================================================================================
    /// <summary>
    /// Clase AppPolicies. Helpers estaticos para construir nombres de politica de autorizacion por modulo o
    /// por permiso.
    /// </summary>
    public static class AppPolicies {
        public const string ModulePrefix = "Module:";
        public const string PermissionPrefix = "Permission:";

        public const string PlanningApprovalPermission = "planning:approve";
        public const string ManufacturingReleasePermission = "manufacturing:release";

        /// <summary>
        /// Construye el nombre de la politica de autorizacion para un modulo dado.
        /// </summary>
        public static string Module(string moduleCode) => $"{ModulePrefix}{moduleCode}";
        /// <summary>
        /// Construye el nombre de la politica de autorizacion para un permiso dado.
        /// </summary>
        public static string Permission(string permissionCode) => $"{PermissionPrefix}{permissionCode}";
    }
}
