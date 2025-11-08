using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ClientesAPI.Data;
using ClientesAPI.Models;
using ClientesAPI.DTOs;
using System.IO.Compression;

namespace ClientesAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ArchivosController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public ArchivosController(ApplicationDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        // POST: api/Archivos/SubirZip
        [HttpPost("SubirZip")]
        public async Task<ActionResult> SubirArchivosZip([FromForm] ArchivoUploadDto archivoDto)
        {
            try
            {
                // Validar que el cliente exista
                var clienteExiste = await _context.Clientes.AnyAsync(c => c.CI == archivoDto.CICliente);
                if (!clienteExiste)
                {
                    return NotFound(new { message = "El cliente no existe" });
                }

                // Validar que sea un archivo ZIP
                if (!archivoDto.ArchivoZip.FileName.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
                {
                    return BadRequest(new { message = "El archivo debe ser un ZIP" });
                }

                // Crear carpeta temporal para descomprimir
                var tempPath = Path.Combine(_environment.ContentRootPath, "Temp", Guid.NewGuid().ToString());
                Directory.CreateDirectory(tempPath);

                // Crear carpeta del cliente en Uploads
                var clienteFolderPath = Path.Combine(_environment.ContentRootPath, "Uploads", archivoDto.CICliente);
                Directory.CreateDirectory(clienteFolderPath);

                // Guardar el ZIP temporalmente
                var zipPath = Path.Combine(tempPath, archivoDto.ArchivoZip.FileName);
                using (var stream = new FileStream(zipPath, FileMode.Create))
                {
                    await archivoDto.ArchivoZip.CopyToAsync(stream);
                }

                // Descomprimir el archivo ZIP
                ZipFile.ExtractToDirectory(zipPath, tempPath);

                // Obtener todos los archivos descomprimidos (excepto el ZIP)
                var archivos = Directory.GetFiles(tempPath, "*.*", SearchOption.AllDirectories)
                                       .Where(f => !f.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
                                       .ToList();

                if (!archivos.Any())
                {
                    // Limpiar archivos temporales
                    Directory.Delete(tempPath, true);
                    return BadRequest(new { message = "El archivo ZIP está vacío" });
                }

                var archivosGuardados = new List<object>();

                // Procesar cada archivo
                foreach (var archivoPath in archivos)
                {
                    var nombreArchivo = Path.GetFileName(archivoPath);
                    var extension = Path.GetExtension(nombreArchivo);

                    // Generar un nombre único para evitar conflictos
                    var nombreUnico = $"{Path.GetFileNameWithoutExtension(nombreArchivo)}_{DateTime.Now:yyyyMMddHHmmss}{extension}";
                    var destinoPath = Path.Combine(clienteFolderPath, nombreUnico);

                    // Copiar el archivo a la carpeta del cliente
                    System.IO.File.Copy(archivoPath, destinoPath, true);

                    // Crear la URL del archivo
                    var urlArchivo = $"/Uploads/{archivoDto.CICliente}/{nombreUnico}";

                    // Registrar en la base de datos
                    var archivoCliente = new ArchivoCliente
                    {
                        CICliente = archivoDto.CICliente,
                        NombreArchivo = nombreArchivo,
                        UrlArchivo = urlArchivo,
                        FechaSubida = DateTime.Now
                    };

                    _context.ArchivosCliente.Add(archivoCliente);
                    await _context.SaveChangesAsync();

                    archivosGuardados.Add(new
                    {
                        idArchivo = archivoCliente.IdArchivo,
                        nombreArchivo = archivoCliente.NombreArchivo,
                        urlArchivo = archivoCliente.UrlArchivo,
                        fechaSubida = archivoCliente.FechaSubida
                    });
                }

                // Limpiar archivos temporales
                Directory.Delete(tempPath, true);

                return Ok(new
                {
                    message = "Archivos subidos exitosamente",
                    cantidadArchivos = archivosGuardados.Count,
                    archivos = archivosGuardados
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al procesar los archivos", error = ex.Message });
            }
        }

        // GET: api/Archivos/Cliente/{ci}
        [HttpGet("Cliente/{ci}")]
        public async Task<ActionResult> ObtenerArchivosPorCliente(string ci)
        {
            try
            {
                var archivos = await _context.ArchivosCliente
                    .Where(a => a.CICliente == ci)
                    .Select(a => new
                    {
                        a.IdArchivo,
                        a.NombreArchivo,
                        a.UrlArchivo,
                        a.FechaSubida
                    })
                    .ToListAsync();

                if (!archivos.Any())
                {
                    return NotFound(new { message = "No se encontraron archivos para este cliente" });
                }

                return Ok(new
                {
                    ciCliente = ci,
                    cantidadArchivos = archivos.Count,
                    archivos = archivos
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al obtener los archivos", error = ex.Message });
            }
        }

        // GET: api/Archivos
        [HttpGet]
        public async Task<ActionResult> ObtenerTodosLosArchivos()
        {
            try
            {
                var archivos = await _context.ArchivosCliente
                    .Include(a => a.Cliente)
                    .Select(a => new
                    {
                        a.IdArchivo,
                        a.CICliente,
                        NombreCliente = a.Cliente.Nombres,
                        a.NombreArchivo,
                        a.UrlArchivo,
                        a.FechaSubida
                    })
                    .ToListAsync();

                return Ok(new
                {
                    cantidadTotal = archivos.Count,
                    archivos = archivos
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al obtener los archivos", error = ex.Message });
            }
        }

        // DELETE: api/Archivos/{id}
        [HttpDelete("{id}")]
        public async Task<ActionResult> EliminarArchivo(int id)
        {
            try
            {
                var archivo = await _context.ArchivosCliente.FindAsync(id);

                if (archivo == null)
                {
                    return NotFound(new { message = "Archivo no encontrado" });
                }

                // Eliminar el archivo físico
                var filePath = Path.Combine(_environment.ContentRootPath, archivo.UrlArchivo.TrimStart('/'));
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }

                // Eliminar el registro de la base de datos
                _context.ArchivosCliente.Remove(archivo);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Archivo eliminado exitosamente" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al eliminar el archivo", error = ex.Message });
            }
        }
    }
}