using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Reflection; 
using System.Security.Claims;
using SistemaVetIng.Models.Extension;
using SistemaVetIng.Servicios.Interfaces;
using SistemaVetIng.ViewsModels;
using SistemaVetIng.Models.Indentity;


namespace SistemaVetIng.Servicios.Implementacion
{
    public class PermissionService : IPermissionService
    {
        private readonly RoleManager<Rol> _roleManager;
        private readonly UserManager<Usuario> _userManager;
       
        public PermissionService(RoleManager<Rol> roleManager, UserManager<Usuario> userManager )
        {
            _roleManager = roleManager;
            _userManager = userManager;
        }

        public async Task<List<Rol>> GetRolesAsync()
        {
            return await _roleManager.Roles.ToListAsync();
        }
        #region Gestion de permisos por grupo
        public async Task<RolPermissionsViewModel> GetRolePermissionsAsync(string roleId)
        {
            var role = await _roleManager.FindByIdAsync(roleId);
            if (role == null)
            {
                
                return null;
            }

            var model = new RolPermissionsViewModel
            {
                RoleId = role.Id.ToString(),
                RoleName = role.Name,
                Permissions = new List<PermissionsViewModel>()
            };

            // Obtenemos permisos
            var allPermissions = Permission.GetAllPermissions();

            // Obtenemos los permisos del rol
            var roleClaims = await _roleManager.GetClaimsAsync(role);
            var rolePermissions = roleClaims.Where(c => c.Type == "Permission") 
                                            .Select(c => c.Value)
                                            .ToList();

            // Comparamos la lista total con los que tiene el rol
            foreach (var permissionValue in allPermissions)
            {
                model.Permissions.Add(new PermissionsViewModel
                {
                    Value = permissionValue,
                    DisplayName = $"{permissionValue.Split('.')[1]} - {permissionValue.Split('.')[2]}", 
                    IsSelected = rolePermissions.Contains(permissionValue) // Marcamos si ya lo tiene
                });
            }

            return model;
        }

        // Composite
        public async Task<bool> UpdateRolePermissionsAsync(RolPermissionsViewModel model)
        {
            var role = await _roleManager.FindByIdAsync(model.RoleId);
            if (role == null) return false;

            // Obtener los permisos del rol
            var currentClaims = await _roleManager.GetClaimsAsync(role);
            var currentPermissionClaims = currentClaims.Where(c => c.Type == "Permission").ToList();

            // eliminar permisos 
            var claimsToRemove = new List<Claim>();
            foreach (var claim in currentPermissionClaims)
            {
                
                if (!model.Permissions.Any(p => p.Value == claim.Value && p.IsSelected))
                {
                    claimsToRemove.Add(claim);
                }
            }

            foreach (var claim in claimsToRemove)
            {
                // Composite.Remove(hoja)
                await _roleManager.RemoveClaimAsync(role, claim);
            }

            // Agregar Permissions que fueron MARCADOS
            foreach (var Permissions in model.Permissions.Where(p => p.IsSelected))
            {
               
                if (!currentPermissionClaims.Any(c => c.Value == Permissions.Value))
                {
                   
                    // Composite.Add(Hoja)
                    var newClaim = new Claim("Permission", Permissions.Value);
                    await _roleManager.AddClaimAsync(role, newClaim);
                }
            }

            return true;
        }
        #endregion


        #region Gestion de permisos por Usuario
        public async Task<UserPermissionsViewModel> GetUserPermissionsAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return null;
            }

            var model = new UserPermissionsViewModel
            {
                UserId = user.Id.ToString(),
                UserName = user.UserName
            };

            // Obtenemos TODOS los permisos 
            var allPermissions = Permission.GetAllPermissions(); 

            // Obtenemos los permisos 
            var userClaims = await _userManager.GetClaimsAsync(user);
            var userPermissions = userClaims.Where(c => c.Type == "Permission")
                                            .Select(c => c.Value)
                                            .ToList();

            // Comparamos la lista total con los que tiene el usuario
            foreach (var permissionValue in allPermissions)
            {
                model.Permissions.Add(new PermissionsViewModel
                {
                    Value = permissionValue,
                    DisplayName = $"{permissionValue.Split('.')[1]} - {permissionValue.Split('.')[2]}",
                    IsSelected = userPermissions.Contains(permissionValue) 
                });
            }

            return model;
        }

        public async Task<bool> UpdateUserPermissionsAsync(UserPermissionsViewModel model)
        {
            var user = await _userManager.FindByIdAsync(model.UserId);
            if (user == null) return false;

            //  Obtener permisos
            var currentClaims = await _userManager.GetClaimsAsync(user);
            var currentPermissionClaims = currentClaims.Where(c => c.Type == "Permission").ToList();

            //  Remover permisos 
            foreach (var claim in currentPermissionClaims)
            {
                if (!model.Permissions.Any(p => p.Value == claim.Value && p.IsSelected))
                {
                   
                    await _userManager.RemoveClaimAsync(user, claim);
                }
            }

            // Agregar permisos 
            foreach (var permission in model.Permissions.Where(p => p.IsSelected))
            {
                if (!currentPermissionClaims.Any(c => c.Value == permission.Value))
                {
                  
                    var newClaim = new Claim("Permission", permission.Value);
                    await _userManager.AddClaimAsync(user, newClaim);
                }
            }

            return true;
        }
        #endregion

        #region Gestion de Roles
        public async Task<UserRolesViewModel> GetUserRolesAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return null;
            }

            var model = new UserRolesViewModel
            {
                UserId = user.Id.ToString(),
                UserName = user.UserName
            };

            var allRoles = await _roleManager.Roles.ToListAsync();
            var userRoleNames = await _userManager.GetRolesAsync(user); 

            foreach (var role in allRoles)
            {
                model.Roles.Add(new RoleViewModel
                {
                    RoleId = role.Id.ToString(),
                    RoleName = role.Name,
                    
                    IsSelected = userRoleNames.Contains(role.Name)
                });
            }
            return model;
        }

        public async Task<bool> UpdateUserRolesAsync(UserRolesViewModel model)
        {
            var user = await _userManager.FindByIdAsync(model.UserId);
            if (user == null) return false;

            // Obtener los roles del usuario
            var currentRoles = await _userManager.GetRolesAsync(user);

            // Roles seleccionados 
            var selectedRoles = model.Roles.Where(r => r.IsSelected).Select(r => r.RoleName).ToList();

            //  Quitar roles 
            var rolesToRemove = currentRoles.Except(selectedRoles).ToList();
            await _userManager.RemoveFromRolesAsync(user, rolesToRemove);

            //  Agregar roles nuevos
            var rolesToAdd = selectedRoles.Except(currentRoles).ToList();
            await _userManager.AddToRolesAsync(user, rolesToAdd);

            return true;
        }
        #endregion

    }
}

