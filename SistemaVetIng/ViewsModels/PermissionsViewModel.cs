namespace SistemaVetIng.ViewsModels
{
    public class PermissionsViewModel
    {
        public string Type { get; set; } = "Permission"; 
        public string Value { get; set; } 
        public string DisplayName { get; set; } 
        public bool IsSelected { get; set; } //  checkbox
    }
}
