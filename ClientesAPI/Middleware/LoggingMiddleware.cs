using ClientesAPI.Data;
using ClientesAPI.Models;
using System.Text;

namespace ClientesAPI.Middleware
{
    public class LoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<LoggingMiddleware> _logger;

        public LoggingMiddleware(RequestDelegate next, ILogger<LoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context, ApplicationDbContext dbContext)
        {
            // Guardar el body original del response
            var originalBodyStream = context.Response.Body;

            try
            {
                // Leer el Request Body
                context.Request.EnableBuffering();
                var requestBody = await ReadRequestBodyAsync(context.Request);

                // Crear un stream temporal para capturar el response
                using var responseBody = new MemoryStream();
                context.Response.Body = responseBody;

                // Variables para el log
                var startTime = DateTime.Now;
                string responseBodyText = string.Empty;
                int statusCode = 200;

                try
                {
                    // Continuar con el siguiente middleware
                    await _next(context);
                    statusCode = context.Response.StatusCode;

                    // Leer el Response Body
                    context.Response.Body.Seek(0, SeekOrigin.Begin);
                    responseBodyText = await new StreamReader(context.Response.Body).ReadToEndAsync();
                    context.Response.Body.Seek(0, SeekOrigin.Begin);

                    // Registrar como Info si fue exitoso
                    await RegistrarLogAsync(dbContext, context, requestBody, responseBodyText, "Info", null);
                }
                catch (Exception ex)
                {
                    // Capturar el error
                    statusCode = 500;
                    responseBodyText = $"{{\"error\": \"{ex.Message}\"}}";

                    // Registrar como Error
                    await RegistrarLogAsync(dbContext, context, requestBody, responseBodyText, "Error", ex.Message);

                    // Re-lanzar la excepción
                    throw;
                }
                finally
                {
                    // Copiar el response al stream original
                    await responseBody.CopyToAsync(originalBodyStream);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en LoggingMiddleware");

                // Asegurar que se devuelva una respuesta al cliente
                context.Response.ContentType = "application/json";
                context.Response.StatusCode = 500;

                var errorResponse = $"{{\"error\": \"Error interno del servidor\", \"detalle\": \"{ex.Message}\"}}";
                await context.Response.WriteAsync(errorResponse);
            }
            finally
            {
                context.Response.Body = originalBodyStream;
            }
        }

        private async Task<string> ReadRequestBodyAsync(HttpRequest request)
        {
            request.Body.Seek(0, SeekOrigin.Begin);
            using var reader = new StreamReader(request.Body, Encoding.UTF8, leaveOpen: true);
            var body = await reader.ReadToEndAsync();
            request.Body.Seek(0, SeekOrigin.Begin);
            return body;
        }

        private async Task RegistrarLogAsync(
            ApplicationDbContext dbContext,
            HttpContext context,
            string requestBody,
            string responseBody,
            string tipoLog,
            string? detalle)
        {
            try
            {
                var log = new LogApi
                {
                    DateTime = DateTime.Now,
                    TipoLog = tipoLog,
                    RequestBody = LimitarTexto(requestBody, 4000),
                    ResponseBody = LimitarTexto(responseBody, 4000),
                    UrlEndpoint = $"{context.Request.Path}{context.Request.QueryString}",
                    MetodoHttp = context.Request.Method,
                    DireccionIp = context.Connection.RemoteIpAddress?.ToString() ?? "Desconocida",
                    Detalle = detalle
                };

                dbContext.LogsApi.Add(log);
                await dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al guardar el log en la base de datos");
            }
        }

        private string LimitarTexto(string texto, int maxLength)
        {
            if (string.IsNullOrEmpty(texto))
                return texto;

            return texto.Length <= maxLength ? texto : texto.Substring(0, maxLength) + "...";
        }
    }
}