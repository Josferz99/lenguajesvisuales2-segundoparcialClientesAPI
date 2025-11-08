using System.ComponentModel.DataAnnotations;

namespace ClientesAPI.DTOs
{
    public class ClienteRegistroDto
    {
        [Required(ErrorMessage = "La CI es obligatoria")]
        [StringLength(20)]
        public string CI { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio")]
        [StringLength(200)]
        public string Nombres { get; set; }

        [Required(ErrorMessage = "La dirección es obligatoria")]
        [StringLength(300)]
        public string Direccion { get; set; }

        [Required(ErrorMessage = "El teléfono es obligatorio")]
        [StringLength(20)]
        public string Telefono { get; set; }

        // Las fotos se reciben como archivos
        public IFormFile? FotoCasa1 { get; set; }
        public IFormFile? FotoCasa2 { get; set; }
        public IFormFile? FotoCasa3 { get; set; }
    }
}