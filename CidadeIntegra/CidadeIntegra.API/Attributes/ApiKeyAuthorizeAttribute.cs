using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace CidadeIntegra.API.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class ApiKeyAuthorizeAttribute : Attribute, IAsyncActionFilter
    {
        private const string HEADER_NAME = "x-api-key";

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<ApiKeyAuthorizeAttribute>>();
            var apiKey = Environment.GetEnvironmentVariable("MIGRATION_API_KEY");

            if (string.IsNullOrEmpty(apiKey))
            {
                logger.LogError("Variável de ambiente MIGRATION_API_KEY não configurada.");
                context.Result = new ContentResult
                {
                    StatusCode = 500,
                    Content = "Server configuration error."
                };
                return;
            }

            if (!context.HttpContext.Request.Headers.TryGetValue(HEADER_NAME, out var extractedKey))
            {
                logger.LogWarning("Tentativa de acesso sem API Key ao endpoint {Path}", context.HttpContext.Request.Path);
                context.Result = new ContentResult
                {
                    StatusCode = 403,
                    Content = "API Key is missing."
                };
                return;
            }

            if (!string.Equals(extractedKey, apiKey))
            {
                logger.LogWarning("Tentativa de acesso com API Key inválida ao endpoint {Path}", context.HttpContext.Request.Path);
                context.Result = new ContentResult
                {
                    StatusCode = 403,
                    Content = "Invalid API Key."
                };
                return;
            }

            logger.LogInformation("Acesso autorizado via API Key ao endpoint {Path}", context.HttpContext.Request.Path);
            await next();
        }
    }
}