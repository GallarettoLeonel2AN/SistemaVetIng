namespace PerrosPeligrososApi.Middleware
{
    public class ApiKeyMiddleware
    {
        private readonly RequestDelegate _next;
        private const string APIKEYNAME = "PERROPELIGROSO-API-KEY";

        public ApiKeyMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Buscar la configuracion inyectada
            var appSettings = context.RequestServices.GetRequiredService<IConfiguration>();
            var apiKeyConfigurada = appSettings.GetValue<string>("ApiSettings:ApiKey");

            // Verificamos si el Header viene en la peticion
            if (!context.Request.Headers.TryGetValue(APIKEYNAME, out var extractedApiKey))
            {
                context.Response.StatusCode = 401; // Unauthorized
                await context.Response.WriteAsync("Falta la API Key de acceso.");
                return;
            }

            // Verificamos si la clave coincide
            if (!apiKeyConfigurada.Equals(extractedApiKey))
            {
                context.Response.StatusCode = 403; // Forbidden
                await context.Response.WriteAsync("API Key no válida.");
                return;
            }

            // Si todo esta bien, dejamos pasar al controlador
            await _next(context);
        }
    }
}
