using System.ComponentModel.DataAnnotations;

// [GPX-DOC-v1] ================================================================================
// DTOs y records usados por el modulo de administracion: resumen de dashboard, opciones de modulo,
// usuarios, roles y resultado de operacion.
// ================================================================================================

namespace GPX.Web.Services {
    /// <summary>
    /// Registro GestionDashboardSummary. DTOs y records usados por el modulo de administracion: resumen de
    /// dashboard, opciones de modulo, usuarios, roles y resultado de operacion.
    /// </summary>
    public sealed record GestionDashboardSummary(
        int UsersCount,
        int RolesCount,
        int ProfilesCount,
        int ModulesCount,
        int RoleClaimsCount,
        int UserClaimsCount);

    /// <summary>
    /// Registro GestionModuleOption. DTOs y records usados por el modulo de administracion: resumen de
    /// dashboard, opciones de modulo, usuarios, roles y resultado de operacion.
    /// </summary>
    public sealed record GestionModuleOption(
        int Id,
        string Code,
        string Name,
        string Route,
        string ParentCode,
        string ParentName,
        int ParentDisplayOrder,
        int DisplayOrder);

    /// <summary>
    /// Registro GestionUserListItem. DTOs y records usados por el modulo de administracion: resumen de
    /// dashboard, opciones de modulo, usuarios, roles y resultado de operacion.
    /// </summary>
    public sealed record GestionUserListItem(
        string Id,
        string FullName,
        string Email,
        string ProfileName,
        bool EmailConfirmed,
        IReadOnlyList<string> Roles,
        IReadOnlyList<string> Permissions);

    /// <summary>
    /// Registro GestionRoleListItem. DTOs y records usados por el modulo de administracion: resumen de
    /// dashboard, opciones de modulo, usuarios, roles y resultado de operacion.
    /// </summary>
    public sealed record GestionRoleListItem(
        string Id,
        string Name,
        int UserCount,
        IReadOnlyList<string> Permissions);

    /// <summary>
    /// Registro GestionRoleUsersItem. DTOs y records usados por el modulo de administracion: resumen de
    /// dashboard, opciones de modulo, usuarios, roles y resultado de operacion.
    /// </summary>
    public sealed record GestionRoleUsersItem(
        string RoleId,
        string RoleName,
        IReadOnlyList<string> Permissions,
        IReadOnlyList<GestionRoleUserItem> Users);

    /// <summary>
    /// Registro GestionRoleUserItem. DTOs y records usados por el modulo de administracion: resumen de
    /// dashboard, opciones de modulo, usuarios, roles y resultado de operacion.
    /// </summary>
    public sealed record GestionRoleUserItem(
        string UserId,
        string FullName,
        string Email,
        string ProfileName);

    /// <summary>
    /// Registro GestionClaimsUserItem. DTOs y records usados por el modulo de administracion: resumen de
    /// dashboard, opciones de modulo, usuarios, roles y resultado de operacion.
    /// </summary>
    public sealed record GestionClaimsUserItem(
        string UserId,
        string FullName,
        string Email,
        string ProfileName,
        IReadOnlyList<string> Roles,
        IReadOnlyList<string> Permissions);

    /// <summary>
    /// Registro GestionClaimsRoleItem. DTOs y records usados por el modulo de administracion: resumen de
    /// dashboard, opciones de modulo, usuarios, roles y resultado de operacion.
    /// </summary>
    public sealed record GestionClaimsRoleItem(
        string RoleId,
        string RoleName,
        int UserCount,
        IReadOnlyList<string> Permissions);

    /// <summary>
    /// Registro GestionClaimsSummary. DTOs y records usados por el modulo de administracion: resumen de
    /// dashboard, opciones de modulo, usuarios, roles y resultado de operacion.
    /// </summary>
    public sealed record GestionClaimsSummary(
        IReadOnlyList<GestionClaimsUserItem> Users,
        IReadOnlyList<GestionClaimsRoleItem> Roles);

    /// <summary>
    /// Registro GestionProfileListItem. DTOs y records usados por el modulo de administracion: resumen de
    /// dashboard, opciones de modulo, usuarios, roles y resultado de operacion.
    /// </summary>
    public sealed record GestionProfileListItem(
        int Id,
        string Name,
        string Description,
        int UserCount,
        IReadOnlyList<string> Modules);

    /// <summary>
    /// Registro GestionProfileModuleGroup. DTOs y records usados por el modulo de administracion: resumen
    /// de dashboard, opciones de modulo, usuarios, roles y resultado de operacion.
    /// </summary>
    public sealed record GestionProfileModuleGroup(
        string ParentCode,
        string ParentName,
        IReadOnlyList<GestionSelectableModule> Modules);

    /// <summary>
    /// Clase GestionOperationResult. DTOs y records usados por el modulo de administracion: resumen de
    /// dashboard, opciones de modulo, usuarios, roles y resultado de operacion.
    /// </summary>
    public sealed class GestionOperationResult {
        public bool Succeeded { get; init; }
        public string Message { get; init; } = string.Empty;
        public IReadOnlyList<string> Errors { get; init; } = [];

        /// <summary>
        /// Success.
        /// </summary>
        public static GestionOperationResult Success(string message) => new() {
            Succeeded = true,
            Message = message
        };

        /// <summary>
        /// Failure.
        /// </summary>
        public static GestionOperationResult Failure(string message, IEnumerable<string>? errors = null) => new() {
            Succeeded = false,
            Message = message,
            Errors = errors?.ToList() ?? []
        };
    }

    /// <summary>
    /// Clase GestionUserEditor. DTOs y records usados por el modulo de administracion: resumen de
    /// dashboard, opciones de modulo, usuarios, roles y resultado de operacion.
    /// </summary>
    public sealed class GestionUserEditor {
        public string? Id { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio.")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "El correo es obligatorio.")]
        [EmailAddress(ErrorMessage = "El correo no tiene un formato valido.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Selecciona un perfil.")]
        public int? ProfileId { get; set; }

        public bool EmailConfirmed { get; set; } = true;
        public string Password { get; set; } = string.Empty;
        public string PermissionsText { get; set; } = string.Empty;
        public List<GestionSelectableRole> Roles { get; set; } = [];
    }

    /// <summary>
    /// Clase GestionSelectableRole. DTOs y records usados por el modulo de administracion: resumen de
    /// dashboard, opciones de modulo, usuarios, roles y resultado de operacion.
    /// </summary>
    public sealed class GestionSelectableRole {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public bool IsSelected { get; set; }
    }

    /// <summary>
    /// Clase GestionRoleEditor. DTOs y records usados por el modulo de administracion: resumen de
    /// dashboard, opciones de modulo, usuarios, roles y resultado de operacion.
    /// </summary>
    public sealed class GestionRoleEditor {
        public string? Id { get; set; }

        [Required(ErrorMessage = "El nombre del rol es obligatorio.")]
        public string Name { get; set; } = string.Empty;

        public string PermissionsText { get; set; } = string.Empty;
    }

    /// <summary>
    /// Clase GestionProfileEditor. DTOs y records usados por el modulo de administracion: resumen de
    /// dashboard, opciones de modulo, usuarios, roles y resultado de operacion.
    /// </summary>
    public sealed class GestionProfileEditor {
        public int? Id { get; set; }

        [Required(ErrorMessage = "El nombre del perfil es obligatorio.")]
        public string Name { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;
    }

    /// <summary>
    /// Clase GestionProfileModulesEditor. DTOs y records usados por el modulo de administracion: resumen de
    /// dashboard, opciones de modulo, usuarios, roles y resultado de operacion.
    /// </summary>
    public sealed class GestionProfileModulesEditor {
        [Required(ErrorMessage = "Selecciona un perfil.")]
        public int? ProfileId { get; set; }

        public string ProfileName { get; set; } = string.Empty;
        public List<GestionSelectableModule> Modules { get; set; } = [];
    }

    /// <summary>
    /// Clase GestionSelectableModule. DTOs y records usados por el modulo de administracion: resumen de
    /// dashboard, opciones de modulo, usuarios, roles y resultado de operacion.
    /// </summary>
    public sealed class GestionSelectableModule {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Route { get; set; } = string.Empty;
        public string ParentCode { get; set; } = string.Empty;
        public string ParentName { get; set; } = string.Empty;
        public int ParentDisplayOrder { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsSelected { get; set; }
        public string RouteLabel => string.IsNullOrWhiteSpace(Route) ? "Sin ruta" : Route;
    }
}
