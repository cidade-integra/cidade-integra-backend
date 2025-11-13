using CidadeIntegra.API.Options;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Options;

namespace CidadeIntegra.API.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class ApiKeyAuthorizeAttribute : Attribute, IAsyncActionFilter
    {
        private const string HEADER_NAME = "x-api-key";

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var logger = context.HttpContext.RequestServices
                .GetRequiredService<ILogger<ApiKeyAuthorizeAttribute>>();

            var options = context.HttpContext.RequestServices
                .GetRequiredService<IOptions<MigrationOptions>>();

            var apiKey = options.Value.ApiKey;

            if (string.IsNullOrEmpty(apiKey))
            {
                logger.LogError("API Key não configurada no servidor.");
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Server configuration error: API Key not set.");
                Console.ResetColor();

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
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"Access denied (missing API Key) at endpoint {context.HttpContext.Request.Path}");
                Console.ResetColor();

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
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"Access denied (invalid API Key) at endpoint {context.HttpContext.Request.Path}");
                Console.ResetColor();

                context.Result = new ContentResult
                {
                    StatusCode = 403,
                    Content = "Invalid API Key."
                };
                return;
            }

            logger.LogInformation("Acesso autorizado via API Key ao endpoint {Path}", context.HttpContext.Request.Path);
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"Access granted to endpoint {context.HttpContext.Request.Path}");
            Console.ResetColor();

            await next();
        }
    }
}