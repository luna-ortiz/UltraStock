using System.ComponentModel.DataAnnotations;

namespace UltraStock.Models
{
    public class Usuario
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string Correo { get; set; }
        public string Rol { get; set; }

        [Required]
        [RegularExpression(@"^3\d{9}$",
            ErrorMessage = "El celular debe estar entre 3000000000 y 3999999999")]
        public string celular { get; set; }
    }
}