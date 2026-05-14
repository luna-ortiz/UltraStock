using System.ComponentModel.DataAnnotations;

namespace UltraStock.Models
{
    public class Usuario
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio")]
        [StringLength(100, ErrorMessage = "Máximo 100 caracteres")]
        public string Nombre { get; set; }

        [Required(ErrorMessage = "El correo es obligatorio")]
        [EmailAddress(ErrorMessage = "Correo inválido")]
        [StringLength(100)]
        public string Correo { get; set; }

        [Required(ErrorMessage = "El rol es obligatorio")]
        [RegularExpression("^(Administrador|EncargadoAlmacen|Supervisor|Auxiliar|Gerente)$",
            ErrorMessage = "Rol no válido")]
        [StringLength(50)]
        public string Rol { get; set; }

        [Required(ErrorMessage = "El celular es obligatorio")]
        [RegularExpression(@"^3\d{9}$",
            ErrorMessage = "El celular debe comenzar con 3 y tener 10 dígitos")]
        public string celular { get; set; }

        [Required(ErrorMessage = "La clave es obligatoria")]
        [MinLength(4, ErrorMessage = "Mínimo 4 caracteres")]
        public string Clave { get; set; }
    }
}
