using Microsoft.AspNetCore.Mvc.Rendering;

public class ManageUserRolesPageViewModel
{
    public string SelectedUserId { get; set; }
    public List<SelectListItem> UsersList { get; set; }
    public UserRolesViewModel RolesForm { get; set; } 

    public ManageUserRolesPageViewModel()
    {
        UsersList = new List<SelectListItem>();
        RolesForm = new UserRolesViewModel();
    }
}