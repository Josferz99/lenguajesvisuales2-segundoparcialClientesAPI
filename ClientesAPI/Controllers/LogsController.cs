using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ClientesAPI.Data;
using ClientesAPI.Models;

namespace ClientesAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LogsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public LogsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Logs
        [HttpGet]
        public async Task<ActionResult> ObtenerTodosLosLogs(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 50)
        {
            try
            {
                var totalLogs = await _context.LogsApi.CountAsync();

                var logs = await _context.LogsApi
                    .OrderByDescending(l => l.DateTime)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .Select(l => new
                    {
                        l.IdLog,
                        l.DateTime,
                        l.TipoLog,
                        l.UrlEndpoint,
                        l.MetodoHttp,
                        l.DireccionIp,
                        l.Detalle,
                        RequestBodyPreview = l.RequestBody != null && l.RequestBody.Length > 100
                            ? l.RequestBody.Substring(0, 100) + "..."
                            : l.RequestBody,
                        ResponseBodyPreview = l.ResponseBody != null && l.ResponseBody.Length > 100
                            ? l.ResponseBody.Substring(0, 100) + "..."
                            : l.ResponseBody
                    })
                    .ToListAsync();

                return Ok(new
                {
                    totalLogs = totalLogs,
                    pageNumber = pageNumber,
                    pageSize = pageSize,
                    totalPages = (int)Math.Ceiling(totalLogs / (double)pageSize),
                    logs = logs
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al obtener los logs", error = ex.Message });
            }
        }

        // GET: api/Logs/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult> ObtenerLogPorId(int id)
        {
            try
            {
                var log = await _context.LogsApi.FindAsync(id);

                if (log == null)
                {
                    return NotFound(new { message = "Log no encontrado" });
                }

                return Ok(log);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al obtener el log", error = ex.Message });
            }
        }

        // GET: api/Logs/Tipo/{tipo}
        [HttpGet("Tipo/{tipo}")]
        public async Task<ActionResult> ObtenerLogsPorTipo(string tipo)
        {
            try
            {
                var logs = await _context.LogsApi
                    .Where(l => l.TipoLog == tipo)
                    .OrderByDescending(l => l.DateTime)
                    .Take(100)
                    .ToListAsync();

                return Ok(new
                {
                    tipo = tipo,
                    cantidad = logs.Count,
                    logs = logs
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al obtener los logs", error = ex.Message });
            }
        }

        // GET: api/Logs/Errores
        [HttpGet("Errores")]
        public async Task<ActionResult> ObtenerSoloErrores()
        {
            try
            {
                var errores = await _context.LogsApi
                    .Where(l => l.TipoLog == "Error")
                    .OrderByDescending(l => l.DateTime)
                    .Take(100)
                    .ToListAsync();

                return Ok(new
                {
                    cantidadErrores = errores.Count,
                    errores = errores
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al obtener los errores", error = ex.Message });
            }
        }

        // GET: api/Logs/Estadisticas
        [HttpGet("Estadisticas")]
        public async Task<ActionResult> ObtenerEstadisticas()
        {
            try
            {
                var totalLogs = await _context.LogsApi.CountAsync();
                var totalErrores = await _context.LogsApi.CountAsync(l => l.TipoLog == "Error");
                var totalInfo = await _context.LogsApi.CountAsync(l => l.TipoLog == "Info");
                var totalWarning = await _context.LogsApi.CountAsync(l => l.TipoLog == "Warning");

                var ultimosErrores = await _context.LogsApi
                    .Where(l => l.TipoLog == "Error")
                    .OrderByDescending(l => l.DateTime)
                    .Take(5)
                    .Select(l => new
                    {
                        l.IdLog,
                        l.DateTime,
                        l.UrlEndpoint,
                        l.Detalle
                    })
                    .ToListAsync();

                return Ok(new
                {
                    totalLogs = totalLogs,
                    totalErrores = totalErrores,
                    totalInfo = totalInfo,
                    totalWarning = totalWarning,
                    ultimosErrores = ultimosErrores
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al obtener las estadísticas", error = ex.Message });
            }
        }

        // DELETE: api/Logs/Limpiar
        [HttpDelete("Limpiar")]
        public async Task<ActionResult> LimpiarLogsAntiguos([FromQuery] int diasAntiguedad = 30)
        {
            try
            {
                var fechaLimite = DateTime.Now.AddDays(-diasAntiguedad);

                var logsAntiguos = await _context.LogsApi
                    .Where(l => l.DateTime < fechaLimite)
                    .ToListAsync();

                _context.LogsApi.RemoveRange(logsAntiguos);
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    message = "Logs antiguos eliminados",
                    cantidadEliminada = logsAntiguos.Count
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al limpiar los logs", error = ex.Message });
            }
        }
    }
}