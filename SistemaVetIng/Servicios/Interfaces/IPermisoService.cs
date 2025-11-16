using Microsoft.AspNetCore.Identity;
using SistemaVetIng.ViewsModels;
using SistemaVetIng.Models.Indentity;


namespace SistemaVetIng.Servicios.Interfaces
{
    public interface IPermissionService
    {
        Task<List<Rol>> GetRolesAsync();

        Task<RolPermissionsViewModel> GetRolePermissionsAsync(string roleId);


        Task<bool> UpdateRolePermissionsAsync(RolPermissionsViewModel model);


        Task<UserPermissionsViewModel> GetUserPermissionsAsync(string userId);
        Task<bool> UpdateUserPermissionsAsync(UserPermissionsViewModel model);


        Task<UserRolesViewModel> GetUserRolesAsync(string userId);
        Task<bool> UpdateUserRolesAsync(UserRolesViewModel model);
    }
}
