using Microsoft.AspNetCore.Mvc.Rendering;
using SistemaVetIng.ViewsModels;
using System.Collections.Generic;

public class ManageUserPermissionsPageViewModel
{
    
    public string SelectedUserId { get; set; }
    public List<SelectListItem> UsersList { get; set; }

   
    public UserPermissionsViewModel PermissionsForm { get; set; }

    public ManageUserPermissionsPageViewModel()
    {
        UsersList = new List<SelectListItem>();
       
        PermissionsForm = new UserPermissionsViewModel();
    }
}