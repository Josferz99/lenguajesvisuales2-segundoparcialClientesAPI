using System.ComponentModel.DataAnnotations;

namespace ClientesAPI.Models
{
    public class LogApi
    {
        [Key]
        public int IdLog { get; set; }

        public DateTime DateTime { get; set; } = DateTime.Now;

        [StringLength(50)]
        public string TipoLog { get; set; } // "Info", "Error", "Warning"

        public string? RequestBody { get; set; }

        public string? ResponseBody { get; set; }

        [StringLength(500)]
        public string? UrlEndpoint { get; set; }

        [StringLength(10)]
        public string? MetodoHttp { get; set; } // GET, POST, PUT, DELETE

        [StringLength(50)]
        public string? DireccionIp { get; set; }

        public string? Detalle { get; set; }
    }
}