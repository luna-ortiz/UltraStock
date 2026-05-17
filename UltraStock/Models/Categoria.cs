using System.ComponentModel.DataAnnotations;
using UltraStock.Models;

namespace UltraStock.Models
{
    public class Categoria
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio")]
        [StringLength(100, ErrorMessage = "Máximo 100 caracteres")]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "La descripción es obligatoria")]
        [StringLength(1000, ErrorMessage = "Máximo 1000 caracteres")]
        public string Descripcion { get; set; } = string.Empty;


        [Required(ErrorMessage = "El estado es obligatorio")]
        [StringLength(100, ErrorMessage = "Máximo 100 caracteres")]
        public string Estado { get; set; } = "Activa";
    }
}
