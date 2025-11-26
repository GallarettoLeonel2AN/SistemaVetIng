public class UserRolesViewModel
{
    public string UserId { get; set; }
    public string UserName { get; set; }
    public List<RoleViewModel> Roles { get; set; }
    public string? UserFullName { get; set; }
    public string? UserRole { get; set; }
    public UserRolesViewModel()
    {
        Roles = new List<RoleViewModel>();
    }
}

// Un sub-modelo simple para el checkbox
public class RoleViewModel
{
    public string RoleId { get; set; }
    public string RoleName { get; set; }
    public bool IsSelected { get; set; }
}