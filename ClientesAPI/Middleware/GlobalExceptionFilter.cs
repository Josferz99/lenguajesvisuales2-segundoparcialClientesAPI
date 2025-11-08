using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using ClientesAPI.Data;
using ClientesAPI.Models;

namespace ClientesAPI.Middleware
{
    public class GlobalExceptionFilter : IExceptionFilter
    {
        private readonly ILogger<GlobalExceptionFilter> _logger;
        private readonly ApplicationDbContext _context;

        public GlobalExceptionFilter(ILogger<GlobalExceptionFilter> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public void OnException(ExceptionContext context)
        {
            _logger.LogError(context.Exception, "Excepción no controlada");

            // Registrar en la base de datos
            try
            {
                var log = new LogApi
                {
                    DateTime = DateTime.Now,
                    TipoLog = "Error",
                    UrlEndpoint = context.HttpContext.Request.Path,
                    MetodoHttp = context.HttpContext.Request.Method,
                    DireccionIp = context.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Desconocida",
                    Detalle = $"{context.Exception.Message}\n\nStackTrace:\n{context.Exception.StackTrace}"
                };

                _context.LogsApi.Add(log);
                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al registrar la excepción en la base de datos");
            }

            // Devolver respuesta al cliente
            context.Result = new ObjectResult(new
            {
                error = "Error interno del servidor",
                message = context.Exception.Message,
                timestamp = DateTime.Now
            })
            {
                StatusCode = 500
            };

            context.ExceptionHandled = true;
        }
    }
}