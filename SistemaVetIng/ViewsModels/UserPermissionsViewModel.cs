namespace SistemaVetIng.ViewsModels
{
    public class UserPermissionsViewModel
    {
        public string UserId { get; set; }
        public string UserName { get; set; }
        public List<PermissionsViewModel> Permissions { get; set; }
        public string? NombreCompleto { get; set; } 
        public string? UserRole { get; set; }
        public bool IsSelected { get; set; } //  checkbox
        public UserPermissionsViewModel()
        {
            Permissions = new List<PermissionsViewModel>();
        }

       
    }
}
