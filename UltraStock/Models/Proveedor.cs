using System.ComponentModel.DataAnnotations;
using UltraStock.Models;

namespace UltraStock.Models
{
    public class Proveedor
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio")]
        [StringLength(100, ErrorMessage = "Máximo 100 caracteres")]
        public string Nombre { get; set; }

        [Required(ErrorMessage = "El teléfono es obligatorio")]
        [RegularExpression(@"^\d{7,15}$", ErrorMessage = "El teléfono debe tener entre 7 y 15 dígitos")]
        public string Telefono { get; set; }

        [Required(ErrorMessage = "El email es obligatorio")]
        [EmailAddress(ErrorMessage = "Email inválido")]
        [StringLength(100)]
        public string Email { get; set; }

        [Required(ErrorMessage = "La dirección es obligatoria")]
        [StringLength(200, ErrorMessage = "Máximo 200 caracteres")]
        public string Direccion { get; set; }

        // Navegación
        public ICollection<Producto> Productos { get; set; } = new List<Producto>();
    }
}
