using System.Security.Claims;
using GPX.Web.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

// [GPX-DOC-v1] ================================================================================
// Servicio central del modulo de administracion de usuarios, roles, perfiles y modulos: listados,
// editores y persistencia.
// ================================================================================================

namespace GPX.Web.Services {
    /// <summary>
    /// Clase GestionManagerService. Servicio central del modulo de administracion de usuarios, roles,
    /// perfiles y modulos: listados, editores y persistencia.
    /// </summary>
    public class GestionManagerService {
        private readonly ApplicationDbContext _dbContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        /// <summary>
        /// Inicializa una nueva instancia de la clase GestionManagerService.
        /// </summary>
        public GestionManagerService(
            ApplicationDbContext dbContext,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager) {
            _dbContext = dbContext;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        /// <summary>
        /// Obtiene el resumen de indicadores mostrado en el panel de administracion.
        /// </summary>
        public async Task<GestionDashboardSummary> GetDashboardSummaryAsync() {
            var roleClaimsCount = await _dbContext.RoleClaims.CountAsync(claim => claim.ClaimType == AppClaimTypes.Permission);
            var userClaimsCount = await _dbContext.UserClaims.CountAsync(claim => claim.ClaimType == AppClaimTypes.Permission);

            return new GestionDashboardSummary(
                await _dbContext.Users.CountAsync(),
                await _dbContext.Roles.CountAsync(),
                await _dbContext.Profiles.CountAsync(),
                await _dbContext.Modules.CountAsync(module => module.IsEnabled),
                roleClaimsCount,
                userClaimsCount);
        }

        /// <summary>
        /// Obtiene la lista de modulos disponibles como opciones de seleccion.
        /// </summary>
        public async Task<IReadOnlyList<GestionModuleOption>> GetModuleOptionsAsync() =>
            await _dbContext.Modules
                .AsNoTracking()
                .Where(module => module.IsEnabled)
                .OrderBy(module => module.ParentDisplayOrder)
                .ThenBy(module => module.DisplayOrder)
                .Select(module => new GestionModuleOption(
                    module.Id,
                    module.Code,
                    module.Name,
                    module.Route,
                    module.ParentCode,
                    module.ParentName,
                    module.ParentDisplayOrder,
                    module.DisplayOrder))
                .ToListAsync();

        /// <summary>
        /// Obtiene la lista de perfiles disponibles como opciones de seleccion.
        /// </summary>
        public async Task<IReadOnlyList<AppProfile>> GetProfileOptionsAsync() =>
            await _dbContext.Profiles
                .AsNoTracking()
                .OrderBy(profile => profile.Name)
                .ToListAsync();

        /// <summary>
        /// Obtiene el listado de usuarios registrados.
        /// </summary>
        public async Task<IReadOnlyList<GestionUserListItem>> GetUsersAsync() {
            var users = await _dbContext.Users
                .AsNoTracking()
                .Include(user => user.Profile)
                .OrderBy(user => user.FullName)
                .ThenBy(user => user.Email)
                .ToListAsync();

            var userIds = users.Select(user => user.Id).ToList();
            var roleMap = await GetUserRolesAsync(userIds);
            var permissionMap = await GetUserPermissionsAsync(userIds);

            return users
                .Select(user => new GestionUserListItem(
                    user.Id,
                    string.IsNullOrWhiteSpace(user.FullName) ? user.Email ?? user.UserName ?? "Usuario" : user.FullName,
                    user.Email ?? string.Empty,
                    user.Profile?.Name ?? "Sin perfil",
                    user.EmailConfirmed,
                    roleMap.GetValueOrDefault(user.Id, []),
                    permissionMap.GetValueOrDefault(user.Id, [])))
                .ToList();
        }

        /// <summary>
        /// Obtiene el listado de roles disponibles.
        /// </summary>
        public async Task<IReadOnlyList<GestionRoleListItem>> GetRolesAsync() {
            var roles = await _dbContext.Roles
                .AsNoTracking()
                .OrderBy(role => role.Name)
                .ToListAsync();

            var roleIds = roles.Select(role => role.Id).ToList();
            var userCounts = await _dbContext.UserRoles
                .Where(link => roleIds.Contains(link.RoleId))
                .GroupBy(link => link.RoleId)
                .Select(group => new { RoleId = group.Key, Count = group.Count() })
                .ToDictionaryAsync(item => item.RoleId, item => item.Count);

            var permissions = await GetRolePermissionsAsync(roleIds);

            return roles
                .Select(role => new GestionRoleListItem(
                    role.Id,
                    role.Name ?? string.Empty,
                    userCounts.GetValueOrDefault(role.Id, 0),
                    permissions.GetValueOrDefault(role.Id, [])))
                .ToList();
        }

        /// <summary>
        /// Obtiene los usuarios agrupados por rol.
        /// </summary>
        public async Task<IReadOnlyList<GestionRoleUsersItem>> GetUsersByRoleAsync() {
            var roles = await _dbContext.Roles
                .AsNoTracking()
                .OrderBy(role => role.Name)
                .ToListAsync();

            var roleIds = roles.Select(role => role.Id).ToList();
            var permissions = await GetRolePermissionsAsync(roleIds);

            var usersByRole = await (
                    from link in _dbContext.UserRoles.AsNoTracking()
                    join user in _dbContext.Users.AsNoTracking().Include(record => record.Profile) on link.UserId equals user.Id
                    where roleIds.Contains(link.RoleId)
                    orderby user.FullName, user.Email
                    select new {
                        link.RoleId,
                        User = new GestionRoleUserItem(
                            user.Id,
                            string.IsNullOrWhiteSpace(user.FullName) ? user.Email ?? user.UserName ?? "Usuario" : user.FullName,
                            user.Email ?? string.Empty,
                            user.Profile != null ? user.Profile.Name : "Sin perfil")
                    })
                .ToListAsync();

            return roles
                .Select(role => new GestionRoleUsersItem(
                    role.Id,
                    role.Name ?? string.Empty,
                    permissions.GetValueOrDefault(role.Id, []),
                    usersByRole.Where(item => item.RoleId == role.Id).Select(item => item.User).ToList()))
                .ToList();
        }

        /// <summary>
        /// Obtiene el listado de perfiles funcionales (AppProfile).
        /// </summary>
        public async Task<IReadOnlyList<GestionProfileListItem>> GetProfilesAsync() {
            var profiles = await _dbContext.Profiles
                .AsNoTracking()
                .Include(profile => profile.Users)
                .Include(profile => profile.ProfileModules)
                .ThenInclude(link => link.Module)
                .OrderBy(profile => profile.Name)
                .ToListAsync();

            return profiles
                .Select(profile => new GestionProfileListItem(
                    profile.Id,
                    profile.Name,
                    profile.Description,
                    profile.Users.Count,
                    profile.ProfileModules
                        .Where(link => link.Module.IsEnabled)
                        .OrderBy(link => link.Module.ParentDisplayOrder)
                        .ThenBy(link => link.Module.DisplayOrder)
                        .Select(link => link.Module.Name)
                        .ToList()))
                .ToList();
        }

        /// <summary>
        /// Obtiene el resumen de claims de permisos sincronizados.
        /// </summary>
        public async Task<GestionClaimsSummary> GetClaimsSummaryAsync() {
            var users = await GetUsersAsync();
            var roles = await GetRolesAsync();

            return new GestionClaimsSummary(
                users.Select(user => new GestionClaimsUserItem(
                    user.Id,
                    user.FullName,
                    user.Email,
                    user.ProfileName,
                    user.Roles,
                    user.Permissions)).ToList(),
                roles.Select(role => new GestionClaimsRoleItem(
                    role.Id,
                    role.Name,
                    role.UserCount,
                    role.Permissions)).ToList());
        }

        /// <summary>
        /// Construye el modelo de edicion de un usuario (nuevo o existente).
        /// </summary>
        public async Task<GestionUserEditor> BuildUserEditorAsync(string? userId = null) {
            var roles = await _dbContext.Roles
                .AsNoTracking()
                .OrderBy(role => role.Name)
                .ToListAsync();

            var editor = new GestionUserEditor {
                Roles = roles.Select(role => new GestionSelectableRole {
                    Id = role.Id,
                    Name = role.Name ?? string.Empty
                }).ToList()
            };

            if(string.IsNullOrWhiteSpace(userId)) {
                return editor;
            }

            var user = await _dbContext.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(record => record.Id == userId);

            if(user == null) {
                return editor;
            }

            var currentRoles = await _userManager.GetRolesAsync(user);
            var currentClaims = await _userManager.GetClaimsAsync(user);

            editor.Id = user.Id;
            editor.FullName = user.FullName;
            editor.Email = user.Email ?? string.Empty;
            editor.ProfileId = user.ProfileId;
            editor.EmailConfirmed = user.EmailConfirmed;
            editor.PermissionsText = JoinPermissions(currentClaims);

            foreach(var role in editor.Roles) {
                role.IsSelected = currentRoles.Contains(role.Name, StringComparer.OrdinalIgnoreCase);
            }

            return editor;
        }

        /// <summary>
        /// Construye el modelo de edicion de un rol (nuevo o existente).
        /// </summary>
        public async Task<GestionRoleEditor> BuildRoleEditorAsync(string? roleId = null) {
            if(string.IsNullOrWhiteSpace(roleId)) {
                return new GestionRoleEditor();
            }

            var role = await _roleManager.FindByIdAsync(roleId);
            if(role == null) {
                return new GestionRoleEditor();
            }

            var claims = await _roleManager.GetClaimsAsync(role);
            return new GestionRoleEditor {
                Id = role.Id,
                Name = role.Name ?? string.Empty,
                PermissionsText = JoinPermissions(claims)
            };
        }

        /// <summary>
        /// Construye el modelo de edicion de un perfil (nuevo o existente).
        /// </summary>
        public async Task<GestionProfileEditor> BuildProfileEditorAsync(int? profileId = null) {
            if(profileId is null) {
                return new GestionProfileEditor();
            }

            var profile = await _dbContext.Profiles
                .AsNoTracking()
                .FirstOrDefaultAsync(record => record.Id == profileId.Value);

            return profile == null
                ? new GestionProfileEditor()
                : new GestionProfileEditor {
                    Id = profile.Id,
                    Name = profile.Name,
                    Description = profile.Description
                };
        }

        /// <summary>
        /// Construye el modelo de edicion de los modulos asignados a un perfil.
        /// </summary>
        public async Task<GestionProfileModulesEditor> BuildProfileModulesEditorAsync(int? profileId) {
            var modules = await GetModuleOptionsAsync();
            var editor = new GestionProfileModulesEditor {
                ProfileId = profileId,
                Modules = modules.Select(module => new GestionSelectableModule {
                    Id = module.Id,
                    Code = module.Code,
                    Name = module.Name,
                    Route = module.Route,
                    ParentCode = module.ParentCode,
                    ParentName = module.ParentName,
                    ParentDisplayOrder = module.ParentDisplayOrder,
                    DisplayOrder = module.DisplayOrder
                }).ToList()
            };

            if(profileId is null) {
                return editor;
            }

            var profile = await _dbContext.Profiles
                .AsNoTracking()
                .Include(record => record.ProfileModules)
                .FirstOrDefaultAsync(record => record.Id == profileId.Value);

            if(profile == null) {
                return editor;
            }

            editor.ProfileName = profile.Name;
            var selectedIds = profile.ProfileModules.Select(link => link.ModuleId).ToHashSet();
            foreach(var module in editor.Modules) {
                module.IsSelected = selectedIds.Contains(module.Id);
            }

            return editor;
        }

        /// <summary>
        /// Guarda (alta o edicion) un usuario.
        /// </summary>
        public async Task<GestionOperationResult> SaveUserAsync(GestionUserEditor editor) {
            var email = editor.Email.Trim();
            var fullName = editor.FullName.Trim();

            if(string.IsNullOrWhiteSpace(editor.Id) && string.IsNullOrWhiteSpace(editor.Password)) {
                return GestionOperationResult.Failure("La contrasena es obligatoria al crear un usuario.");
            }

            ApplicationUser? user = null;
            var isNew = string.IsNullOrWhiteSpace(editor.Id);

            if(!isNew) {
                user = await _userManager.FindByIdAsync(editor.Id!);
                if(user == null) {
                    return GestionOperationResult.Failure("No fue posible localizar el usuario seleccionado.");
                }
            }

            user ??= new ApplicationUser();
            user.UserName = email;
            user.Email = email;
            user.FullName = fullName;
            user.EmailConfirmed = editor.EmailConfirmed;
            user.ProfileId = editor.ProfileId;

            IdentityResult result;
            if(isNew) {
                result = await _userManager.CreateAsync(user, editor.Password.Trim());
            } else {
                result = await _userManager.UpdateAsync(user);
            }

            if(!result.Succeeded) {
                return GestionOperationResult.Failure("No fue posible guardar el usuario.", result.Errors.Select(error => error.Description));
            }

            if(!isNew && !string.IsNullOrWhiteSpace(editor.Password)) {
                var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
                var resetResult = await _userManager.ResetPasswordAsync(user, resetToken, editor.Password.Trim());
                if(!resetResult.Succeeded) {
                    return GestionOperationResult.Failure("No fue posible actualizar la contrasena del usuario.", resetResult.Errors.Select(error => error.Description));
                }
            }

            var selectedRoles = editor.Roles
                .Where(role => role.IsSelected)
                .Select(role => role.Name)
                .ToArray();

            var currentRoles = await _userManager.GetRolesAsync(user);
            var rolesToAdd = selectedRoles.Except(currentRoles, StringComparer.OrdinalIgnoreCase).ToArray();
            var rolesToRemove = currentRoles.Except(selectedRoles, StringComparer.OrdinalIgnoreCase).ToArray();

            if(rolesToAdd.Length > 0) {
                var addRolesResult = await _userManager.AddToRolesAsync(user, rolesToAdd);
                if(!addRolesResult.Succeeded) {
                    return GestionOperationResult.Failure("No fue posible asignar los roles del usuario.", addRolesResult.Errors.Select(error => error.Description));
                }
            }

            if(rolesToRemove.Length > 0) {
                var removeRolesResult = await _userManager.RemoveFromRolesAsync(user, rolesToRemove);
                if(!removeRolesResult.Succeeded) {
                    return GestionOperationResult.Failure("No fue posible actualizar los roles del usuario.", removeRolesResult.Errors.Select(error => error.Description));
                }
            }

            var syncClaimsResult = await SyncPermissionClaimsAsync(
                getClaims: () => _userManager.GetClaimsAsync(user),
                addClaim: claim => _userManager.AddClaimAsync(user, claim),
                removeClaim: claim => _userManager.RemoveClaimAsync(user, claim),
                desiredPermissions: ParsePermissions(editor.PermissionsText));

            return syncClaimsResult.Succeeded
                ? GestionOperationResult.Success(isNew ? "Usuario creado correctamente." : "Usuario actualizado correctamente.")
                : syncClaimsResult;
        }

        /// <summary>
        /// Guarda (alta o edicion) un rol.
        /// </summary>
        public async Task<GestionOperationResult> SaveRoleAsync(GestionRoleEditor editor) {
            var roleName = editor.Name.Trim();
            var isNew = string.IsNullOrWhiteSpace(editor.Id);
            IdentityRole? role = null;

            if(!isNew) {
                role = await _roleManager.FindByIdAsync(editor.Id!);
                if(role == null) {
                    return GestionOperationResult.Failure("No fue posible localizar el rol seleccionado.");
                }
            }

            role ??= new IdentityRole();
            role.Name = roleName;
            role.NormalizedName = roleName.ToUpperInvariant();

            IdentityResult result = isNew
                ? await _roleManager.CreateAsync(role)
                : await _roleManager.UpdateAsync(role);

            if(!result.Succeeded) {
                return GestionOperationResult.Failure("No fue posible guardar el rol.", result.Errors.Select(error => error.Description));
            }

            var syncClaimsResult = await SyncPermissionClaimsAsync(
                getClaims: () => _roleManager.GetClaimsAsync(role),
                addClaim: claim => _roleManager.AddClaimAsync(role, claim),
                removeClaim: claim => _roleManager.RemoveClaimAsync(role, claim),
                desiredPermissions: ParsePermissions(editor.PermissionsText));

            return syncClaimsResult.Succeeded
                ? GestionOperationResult.Success(isNew ? "Rol creado correctamente." : "Rol actualizado correctamente.")
                : syncClaimsResult;
        }

        /// <summary>
        /// Guarda (alta o edicion) un perfil.
        /// </summary>
        public async Task<GestionOperationResult> SaveProfileAsync(GestionProfileEditor editor) {
            var name = editor.Name.Trim();
            var description = editor.Description.Trim();

            var duplicated = await _dbContext.Profiles
                .AnyAsync(profile => profile.Name == name && profile.Id != (editor.Id ?? 0));

            if(duplicated) {
                return GestionOperationResult.Failure("Ya existe un perfil con ese nombre.");
            }

            if(editor.Id is null) {
                _dbContext.Profiles.Add(new AppProfile {
                    Name = name,
                    Description = description
                });
            } else {
                var profile = await _dbContext.Profiles.FirstOrDefaultAsync(record => record.Id == editor.Id.Value);
                if(profile == null) {
                    return GestionOperationResult.Failure("No fue posible localizar el perfil seleccionado.");
                }

                profile.Name = name;
                profile.Description = description;
            }

            await _dbContext.SaveChangesAsync();
            return GestionOperationResult.Success(editor.Id is null ? "Perfil creado correctamente." : "Perfil actualizado correctamente.");
        }

        /// <summary>
        /// Guarda la asignacion de modulos de un perfil.
        /// </summary>
        public async Task<GestionOperationResult> SaveProfileModulesAsync(GestionProfileModulesEditor editor) {
            if(editor.ProfileId is null) {
                return GestionOperationResult.Failure("Selecciona un perfil para guardar su mapa de modulos.");
            }

            var profile = await _dbContext.Profiles
                .Include(record => record.ProfileModules)
                .FirstOrDefaultAsync(record => record.Id == editor.ProfileId.Value);

            if(profile == null) {
                return GestionOperationResult.Failure("No fue posible localizar el perfil seleccionado.");
            }

            var selectedModuleIds = editor.Modules
                .Where(module => module.IsSelected)
                .Select(module => module.Id)
                .ToHashSet();

            var currentModuleIds = profile.ProfileModules
                .Select(link => link.ModuleId)
                .ToHashSet();

            foreach(var moduleId in selectedModuleIds.Except(currentModuleIds)) {
                profile.ProfileModules.Add(new AppProfileModule {
                    ProfileId = profile.Id,
                    ModuleId = moduleId
                });
            }

            var linksToRemove = profile.ProfileModules
                .Where(link => !selectedModuleIds.Contains(link.ModuleId))
                .ToList();

            if(linksToRemove.Count > 0) {
                _dbContext.ProfileModules.RemoveRange(linksToRemove);
            }

            await _dbContext.SaveChangesAsync();
            return GestionOperationResult.Success("Modulos del perfil actualizados correctamente.");
        }

        /// <summary>
        /// Agrupa los modulos por su modulo padre para la navegacion.
        /// </summary>
        public IReadOnlyList<GestionProfileModuleGroup> GroupModules(GestionProfileModulesEditor editor) =>
            editor.Modules
                .GroupBy(module => new { module.ParentCode, module.ParentName, module.ParentDisplayOrder })
                .OrderBy(group => group.Key.ParentDisplayOrder)
                .Select(group => new GestionProfileModuleGroup(
                    group.Key.ParentCode,
                    group.Key.ParentName,
                    group.OrderBy(module => module.DisplayOrder).ToList()))
                .ToList();

        private async Task<Dictionary<string, IReadOnlyList<string>>> GetUserRolesAsync(IReadOnlyCollection<string> userIds) {
            var userRoles = await (
                    from userRole in _dbContext.UserRoles.AsNoTracking()
                    join role in _dbContext.Roles.AsNoTracking() on userRole.RoleId equals role.Id
                    where userIds.Contains(userRole.UserId)
                    select new { userRole.UserId, RoleName = role.Name ?? string.Empty })
                .ToListAsync();

            return userRoles
                .GroupBy(item => item.UserId)
                .ToDictionary(
                    group => group.Key,
                    group => (IReadOnlyList<string>)group.Select(item => item.RoleName).OrderBy(value => value).ToList());
        }

        private async Task<Dictionary<string, IReadOnlyList<string>>> GetUserPermissionsAsync(IReadOnlyCollection<string> userIds) {
            var userClaims = await _dbContext.UserClaims
                .AsNoTracking()
                .Where(claim => userIds.Contains(claim.UserId) && claim.ClaimType == AppClaimTypes.Permission)
                .Select(claim => new { claim.UserId, Permission = claim.ClaimValue ?? string.Empty })
                .ToListAsync();

            return userClaims
                .GroupBy(item => item.UserId)
                .ToDictionary(
                    group => group.Key,
                    group => (IReadOnlyList<string>)group.Select(item => item.Permission).OrderBy(value => value).ToList());
        }

        private async Task<Dictionary<string, IReadOnlyList<string>>> GetRolePermissionsAsync(IReadOnlyCollection<string> roleIds) {
            var roleClaims = await _dbContext.RoleClaims
                .AsNoTracking()
                .Where(claim => roleIds.Contains(claim.RoleId) && claim.ClaimType == AppClaimTypes.Permission)
                .Select(claim => new { claim.RoleId, Permission = claim.ClaimValue ?? string.Empty })
                .ToListAsync();

            return roleClaims
                .GroupBy(item => item.RoleId)
                .ToDictionary(
                    group => group.Key,
                    group => (IReadOnlyList<string>)group.Select(item => item.Permission).OrderBy(value => value).ToList());
        }

        /// <summary>
        /// Combina una coleccion de codigos de permiso en una cadena de texto.
        /// </summary>
        private static string JoinPermissions(IEnumerable<Claim> claims) =>
            string.Join(Environment.NewLine, claims
                .Where(claim => claim.Type == AppClaimTypes.Permission)
                .Select(claim => claim.Value)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(value => value));

        /// <summary>
        /// Separa una cadena de texto de permisos en su coleccion de codigos.
        /// </summary>
        private static IReadOnlyList<string> ParsePermissions(string permissionsText) =>
            permissionsText
                .Split(["\r\n", "\n", ",", ";"], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(value => value)
                .ToList();

        /// <summary>
        /// Sincroniza los claims de permisos con la configuracion actual de un usuario o rol.
        /// </summary>
        private static async Task<GestionOperationResult> SyncPermissionClaimsAsync(
            Func<Task<IList<Claim>>> getClaims,
            Func<Claim, Task<IdentityResult>> addClaim,
            Func<Claim, Task<IdentityResult>> removeClaim,
            IReadOnlyList<string> desiredPermissions) {
            var currentClaims = await getClaims();
            var currentPermissions = currentClaims
                .Where(claim => claim.Type == AppClaimTypes.Permission)
                .ToList();

            foreach(var claim in currentPermissions.Where(claim => !desiredPermissions.Contains(claim.Value, StringComparer.OrdinalIgnoreCase))) {
                var removeResult = await removeClaim(claim);
                if(!removeResult.Succeeded) {
                    return GestionOperationResult.Failure("No fue posible remover un claim.", removeResult.Errors.Select(error => error.Description));
                }
            }

            foreach(var permission in desiredPermissions.Where(permission => !currentPermissions.Any(claim => string.Equals(claim.Value, permission, StringComparison.OrdinalIgnoreCase)))) {
                var addResult = await addClaim(new Claim(AppClaimTypes.Permission, permission));
                if(!addResult.Succeeded) {
                    return GestionOperationResult.Failure("No fue posible agregar un claim.", addResult.Errors.Select(error => error.Description));
                }
            }

            return GestionOperationResult.Success("Claims sincronizados correctamente.");
        }
    }
}
