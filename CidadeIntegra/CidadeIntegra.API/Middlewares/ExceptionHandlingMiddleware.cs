using System.Net;

namespace CidadeIntegra.API.Middlewares
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context); // executa o próximo middleware
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ocorreu um erro inesperado.");

                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                context.Response.ContentType = "application/json";

                var response = new
                {
                    StatusCode = context.Response.StatusCode,
                    Message = "Erro interno no servidor. Contate o administrador.",
                    Details = ex.Message // mostrar apenas em dev
                };

                await context.Response.WriteAsJsonAsync(response);
            }
        }
    }
}
