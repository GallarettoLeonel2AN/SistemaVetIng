namespace SistemaVetIng.ViewsModels
{
    public class RolPermissionsViewModel
    {
        public string RoleId { get; set; }
        public string RoleName { get; set; }
        public List<PermissionsViewModel> Permissions { get; set; }

        public RolPermissionsViewModel()
        {
            Permissions = new List<PermissionsViewModel>();
        }
    }
}
