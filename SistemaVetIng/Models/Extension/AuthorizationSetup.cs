
using SistemaVetIng.Models.Extension; 
using Microsoft.Extensions.DependencyInjection; 
using Microsoft.AspNetCore.Authorization; 

namespace SistemaVetIng.Extensions 
{
    public static class AuthorizationSetup
    {
        public static IServiceCollection AddAppAuthorization(this IServiceCollection services)
        {
            
            services.AddAuthorization(options =>
            {
                // ATENCIONES 
                options.AddPolicy(Permission.Atenciones.Create, policy =>
                    policy.RequireClaim("Permission", Permission.Atenciones.Create));

                options.AddPolicy(Permission.Atenciones.View, policy =>
                    policy.RequireClaim("Permission", Permission.Atenciones.View));

                //  PAGO 
              
                options.AddPolicy(Permission.Pago.View, policy =>
                    policy.RequireClaim("Permission", Permission.Pago.View));

                // HORARIO 
                options.AddPolicy(Permission.Horario.Create, policy =>
                    policy.RequireClaim("Permission", Permission.Horario.Create));
                options.AddPolicy(Permission.Horario.View, policy =>
                   policy.RequireClaim("Permission", Permission.Horario.View));
  

                //  CLIENTE 
                options.AddPolicy(Permission.Cliente.View, policy =>
                    policy.RequireClaim("Permission", Permission.Cliente.View));
                options.AddPolicy(Permission.Cliente.Create, policy =>
                    policy.RequireClaim("Permission", Permission.Cliente.Create));
                options.AddPolicy(Permission.Cliente.Edit, policy =>
                    policy.RequireClaim("Permission", Permission.Cliente.Edit));
                options.AddPolicy(Permission.Cliente.Delete, policy =>
                    policy.RequireClaim("Permission", Permission.Cliente.Delete));

                //  MASCOTA 
                options.AddPolicy(Permission.Mascota.View, policy =>
                   policy.RequireClaim("Permission", Permission.Mascota.View));
                options.AddPolicy(Permission.Mascota.Create, policy =>
                    policy.RequireClaim("Permission", Permission.Mascota.Create));
                options.AddPolicy(Permission.Mascota.Edit, policy =>
                    policy.RequireClaim("Permission", Permission.Mascota.Edit));
                options.AddPolicy(Permission.Mascota.Delete, policy =>
                    policy.RequireClaim("Permission", Permission.Mascota.Delete));

                //  VETERINARIO
                options.AddPolicy(Permission.Veterinario.View, policy =>
                   policy.RequireClaim("Permission", Permission.Veterinario.View));
                options.AddPolicy(Permission.Veterinario.Create, policy =>
                    policy.RequireClaim("Permission", Permission.Veterinario.Create));
                options.AddPolicy(Permission.Veterinario.Edit, policy =>
                    policy.RequireClaim("Permission", Permission.Veterinario.Edit));
                options.AddPolicy(Permission.Veterinario.Delete, policy =>
                    policy.RequireClaim("Permission", Permission.Veterinario.Delete));
               
                // Turnos 
                options.AddPolicy(Permission.Turnos.View, policy =>
                   policy.RequireClaim("Permission", Permission.Turnos.View));
                options.AddPolicy(Permission.Turnos.Create, policy =>
                    policy.RequireClaim("Permission", Permission.Turnos.Create));
               
                options.AddPolicy(Permission.Turnos.Cancel, policy =>
                    policy.RequireClaim("Permission", Permission.Turnos.Cancel));
                
                //  Adm 
                options.AddPolicy(Permission.Administration.ViewRoles, policy =>
                   policy.RequireClaim("Permission", Permission.Administration.ViewRoles));
                options.AddPolicy(Permission.Administration.ManageRolePermissions, policy =>
                    policy.RequireClaim("Permission", Permission.Administration.ManageRolePermissions));
                options.AddPolicy(Permission.Administration.ViewUsers, policy =>
                    policy.RequireClaim("Permission", Permission.Administration.ViewUsers));
                options.AddPolicy(Permission.Administration.ManageUsers, policy =>
                    policy.RequireClaim("Permission", Permission.Administration.ManageUsers));

                //  Estudios 
                options.AddPolicy(Permission.Estudio.View, policy =>
                   policy.RequireClaim("Permission", Permission.Estudio.View));
                options.AddPolicy(Permission.Estudio.Create, policy =>
                    policy.RequireClaim("Permission", Permission.Estudio.Create));
                options.AddPolicy(Permission.Estudio.Edit, policy =>
                    policy.RequireClaim("Permission", Permission.Estudio.Edit));
                options.AddPolicy(Permission.Estudio.Delete, policy =>
                    policy.RequireClaim("Permission", Permission.Estudio.Delete));

                // Vacuna 
                options.AddPolicy(Permission.Vacuna.View, policy =>
                   policy.RequireClaim("Permission", Permission.Vacuna.View));
                options.AddPolicy(Permission.Vacuna.Create, policy =>
                    policy.RequireClaim("Permission", Permission.Vacuna.Create));
                options.AddPolicy(Permission.Vacuna.Edit, policy =>
                    policy.RequireClaim("Permission", Permission.Vacuna.Edit));
                options.AddPolicy(Permission.Vacuna.Delete, policy =>
                    policy.RequireClaim("Permission", Permission.Vacuna.Delete));

                // Auditoria
                options.AddPolicy(Permission.Auditoria.View, policy =>
                   policy.RequireClaim("Permission", Permission.Auditoria.View));

                // Reporte
                options.AddPolicy(Permission.Dashboard.View, policy =>
                   policy.RequireClaim("Permission", Permission.Dashboard.View));
            });

           
            return services;
        }
    }
}