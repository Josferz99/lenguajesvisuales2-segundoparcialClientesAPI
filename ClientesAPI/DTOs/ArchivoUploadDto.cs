using System.ComponentModel.DataAnnotations;

namespace ClientesAPI.DTOs
{
    public class ArchivoUploadDto
    {
        [Required(ErrorMessage = "La CI del cliente es obligatoria")]
        public string CICliente { get; set; }

        [Required(ErrorMessage = "Debe enviar un archivo ZIP")]
        public IFormFile ArchivoZip { get; set; }
    }
}