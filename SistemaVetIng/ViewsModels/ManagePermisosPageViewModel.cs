using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;


namespace SistemaVetIng.ViewsModels
{
    public class ManagePermissionsPageViewModel
    {
        public string SelectedRoleId { get; set; }
        public List<SelectListItem> RolesList { get; set; }

        
        public RolPermissionsViewModel PermissionsForm { get; set; }

        public ManagePermissionsPageViewModel()
        {
            RolesList = new List<SelectListItem>();
            PermissionsForm = new RolPermissionsViewModel();
        }
    }
}
