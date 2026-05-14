using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using UltraStock.Models;

namespace UltraStock.Models
{
    public class MovimientoInventario
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El producto es obligatorio")]
        public int ProductoId { get; set; }

        [ForeignKey("ProductoId")]
        public Producto? Producto { get; set; }

        [Required(ErrorMessage = "El tipo de movimiento es obligatorio")]
        [RegularExpression("^(Entrada|Salida)$", ErrorMessage = "Debe ser 'Entrada' o 'Salida'")]
        public string TipoMovimiento { get; set; } = string.Empty;

        [Required(ErrorMessage = "La cantidad es obligatoria")]
        [Range(1, int.MaxValue, ErrorMessage = "La cantidad debe ser mayor a 0")]
        public int Cantidad { get; set; }

        [Required]
        public DateTime Fecha { get; set; } = DateTime.Now;

        [StringLength(500, ErrorMessage = "Máximo 500 caracteres")]
        public string? Observacion { get; set; }

    }
}
