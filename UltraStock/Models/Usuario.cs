using System.ComponentModel.DataAnnotations;

namespace UltraStock.Models
{
    public class Usuario
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio")]
        [StringLength(100)]
        public string Nombre { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio")]
        [EmailAddress(ErrorMessage = "Correo inválido")]
        [StringLength(100)]
        public string Correo { get; set; }

        [Required]
        [StringLength(100)]
        public string Rol { get; set; }

        [Required]
        [RegularExpression(@"^3\d{9}$",
            ErrorMessage = "El celular debe estar entre 3000000000 y 3999999999")]
        public string celular { get; set; }

        [Required]
        [MinLength(4, ErrorMessage = "Mínimo 4 caracteres")]
        public string Clave { get; set; }
    }
}