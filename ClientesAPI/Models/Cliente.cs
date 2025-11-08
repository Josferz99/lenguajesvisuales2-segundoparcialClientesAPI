using System.ComponentModel.DataAnnotations;

namespace ClientesAPI.Models
{
    public class Cliente
    {
        [Key]
        public string CI { get; set; } // Cédula de Identidad (Primary Key)

        [Required]
        [StringLength(200)]
        public string Nombres { get; set; }

        [Required]
        [StringLength(300)]
        public string Direccion { get; set; }

        [Required]
        [StringLength(20)]
        public string Telefono { get; set; }

        // Fotos guardadas como bytes (varbinary en SQL)
        public byte[]? FotoCasa1 { get; set; }
        public byte[]? FotoCasa2 { get; set; }
        public byte[]? FotoCasa3 { get; set; }

        // Relación con archivos
        public virtual ICollection<ArchivoCliente> Archivos { get; set; } = new List<ArchivoCliente>();
    }
}