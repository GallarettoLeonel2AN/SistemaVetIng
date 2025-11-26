using System.Reflection;

namespace SistemaVetIng.Models.Extension
{
    public static class Permission
    {
        // Patron Composite
     
        public static class Atenciones
        {
            public const string Create = "Permission.Atenciones.Create";
            public const string View = "Permission.Atenciones.View";
            
        }
        public static class Pago
        {
          
            public const string View = "Permission.Pago.View";

        }

        public static class Auditoria
        {
            public const string View = "Permission.Auditoria.View";
        }

        public static class Dashboard
        {
            public const string View = "Permission.Dashboard.View";
        }

        public static class Horario
        {
            public const string Create = "Permission.Horario.Create";
            public const string View = "Permission.Horario.View";

        }

       
        public static class Cliente
        {
            public const string View = "Permission.Cliente.View";
            public const string Create = "Permission.Cliente.Create";
            public const string Edit = "Permission.Cliente.Edit";
            public const string Delete = "Permission.Cliente.Delete";
        }
        public static class Mascota
        {
            public const string View = "Permission.Mascota.View";
            public const string Create = "Permission.Mascota.Create";
            public const string Edit = "Permission.Mascota.Edit";
            public const string Delete = "Permission.Mascota.Delete";
        }
        public static class Veterinario
        {
            public const string View = "Permission.Veterinario.View";
            public const string Create = "Permission.Veterinario.Create";
            public const string Edit = "Permission.Veterinario.Edit";
            public const string Delete = "Permission.Veterinario.Delete";
        }
        public static class Turnos
        {
            public const string View = "Permission.Turnos.View";
            public const string Create = "Permission.Turnos.Create";
          
            public const string Cancel = "Permission.Turnos.Cancel";
        }

        // Grupo de Permissions para Administración (Roles, Usuarios)
        public static class Administration
        {
            public const string ViewRoles = "Permission.Administration.ViewRoles";
            public const string ManageRolePermissions = "Permission.Administration.ManageRolePermissions";
            public const string ViewUsers = "Permission.Administration.ViewUsers";
            public const string ManageUsers = "Permission.Administration.ManageUsers";
        }

        public static class Estudio
        {
            public const string View = "Permission.Estudio.View";
            public const string Create = "Permission.Estudio.Create";
            public const string Edit = "Permission.Estudio.Edit";
            public const string Delete = "Permission.Estudio.Delete";
        }

        public static class Vacuna
        {
            public const string View = "Permission.Vacuna.View";
            public const string Create = "Permission.Vacuna.Create";
            public const string Edit = "Permission.Vacuna.Edit";
            public const string Delete = "Permission.Vacuna.Delete";
        }
        public static List<string> GetAllPermissions()
        {
            var allPermissions = new List<string>();

            // Obtenemos todas las clases anidadas 
            var nestedClasses = typeof(Permission).GetNestedTypes(BindingFlags.Public | BindingFlags.Static);

            foreach (var type in nestedClasses)
            {
                // Obtenemos todos los campos constantes (View, Create, Edit, etc.)
                var fields = type.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
                                 .Where(fi => fi.IsLiteral && !fi.IsInitOnly && fi.FieldType == typeof(string));

                foreach (var field in fields)
                {
                    
                    allPermissions.Add((string)field.GetRawConstantValue());
                }
            }

            return allPermissions;
        }

    }
}
