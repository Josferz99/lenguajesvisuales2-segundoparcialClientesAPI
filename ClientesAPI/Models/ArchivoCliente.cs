using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ClientesAPI.Models
{
    public class ArchivoCliente
    {
        [Key]
        public int IdArchivo { get; set; }

        [Required]
        [StringLength(20)]
        public string CICliente { get; set; } // Foreign Key

        [Required]
        [StringLength(300)]
        public string NombreArchivo { get; set; }

        [Required]
        [StringLength(500)]
        public string UrlArchivo { get; set; }

        public DateTime FechaSubida { get; set; } = DateTime.Now;

        // Navegación
        [ForeignKey("CICliente")]
        public virtual Cliente Cliente { get; set; }
    }
}