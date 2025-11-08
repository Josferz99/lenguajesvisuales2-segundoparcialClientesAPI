using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ClientesAPI.Data;
using ClientesAPI.Models;
using ClientesAPI.DTOs;

namespace ClientesAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ClientesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ClientesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // POST: api/Clientes
        [HttpPost]
        public async Task<ActionResult<Cliente>> RegistrarCliente([FromForm] ClienteRegistroDto clienteDto)
        {
            try
            {
                // Validar que no exista el cliente
                if (await _context.Clientes.AnyAsync(c => c.CI == clienteDto.CI))
                {
                    return BadRequest(new { message = "Ya existe un cliente con esa CI" });
                }

                // Crear el objeto Cliente
                var cliente = new Cliente
                {
                    CI = clienteDto.CI,
                    Nombres = clienteDto.Nombres,
                    Direccion = clienteDto.Direccion,
                    Telefono = clienteDto.Telefono
                };

                // Convertir las fotos a bytes si fueron enviadas
                if (clienteDto.FotoCasa1 != null)
                {
                    using var ms1 = new MemoryStream();
                    await clienteDto.FotoCasa1.CopyToAsync(ms1);
                    cliente.FotoCasa1 = ms1.ToArray();
                }

                if (clienteDto.FotoCasa2 != null)
                {
                    using var ms2 = new MemoryStream();
                    await clienteDto.FotoCasa2.CopyToAsync(ms2);
                    cliente.FotoCasa2 = ms2.ToArray();
                }

                if (clienteDto.FotoCasa3 != null)
                {
                    using var ms3 = new MemoryStream();
                    await clienteDto.FotoCasa3.CopyToAsync(ms3);
                    cliente.FotoCasa3 = ms3.ToArray();
                }

                // Guardar en la base de datos
                _context.Clientes.Add(cliente);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(ObtenerCliente), new { ci = cliente.CI }, new
                {
                    message = "Cliente registrado exitosamente",
                    cliente = new
                    {
                        cliente.CI,
                        cliente.Nombres,
                        cliente.Direccion,
                        cliente.Telefono,
                        FotoCasa1 = cliente.FotoCasa1 != null ? "Imagen guardada" : "No enviada",
                        FotoCasa2 = cliente.FotoCasa2 != null ? "Imagen guardada" : "No enviada",
                        FotoCasa3 = cliente.FotoCasa3 != null ? "Imagen guardada" : "No enviada"
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al registrar el cliente", error = ex.Message });
            }
        }

        // GET: api/Clientes/{ci}
        [HttpGet("{ci}")]
        public async Task<ActionResult<Cliente>> ObtenerCliente(string ci)
        {
            var cliente = await _context.Clientes.FindAsync(ci);

            if (cliente == null)
            {
                return NotFound(new { message = "Cliente no encontrado" });
            }

            return Ok(new
            {
                cliente.CI,
                cliente.Nombres,
                cliente.Direccion,
                cliente.Telefono,
                TieneFoto1 = cliente.FotoCasa1 != null,
                TieneFoto2 = cliente.FotoCasa2 != null,
                TieneFoto3 = cliente.FotoCasa3 != null
            });
        }

        // GET: api/Clientes
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Cliente>>> ObtenerTodosClientes()
        {
            var clientes = await _context.Clientes
                .Select(c => new
                {
                    c.CI,
                    c.Nombres,
                    c.Direccion,
                    c.Telefono,
                    TieneFoto1 = c.FotoCasa1 != null,
                    TieneFoto2 = c.FotoCasa2 != null,
                    TieneFoto3 = c.FotoCasa3 != null
                })
                .ToListAsync();

            return Ok(clientes);
        }
    }
}