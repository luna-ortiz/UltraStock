using System.ComponentModel.DataAnnotations;
using UltraStock.Models;
using System.ComponentModel.DataAnnotations.Schema;

namespace UltraStock.Models
{
    public class Producto
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio")]
        [StringLength(100, ErrorMessage = "Máximo 100 caracteres")]
        public string Nombre { get; set; }

        [StringLength(500, ErrorMessage = "Máximo 500 caracteres")]
        public string? Descripcion { get; set; }

        [Required(ErrorMessage = "El precio es obligatorio")]
        [Range(0, 1000000, ErrorMessage = "El precio debe estar entre 0 y 1.000.000")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Precio { get; set; }

        [Range(0, 100000, ErrorMessage = "El stock debe estar entre 0 y 100.000")]
        public int Stock { get; set; }

        [Required(ErrorMessage = "La categoría es obligatoria")]
        public int CategoriaId { get; set; }

        [ForeignKey("CategoriaId")]
        public Categoria? Categoria { get; set; }

        [Required(ErrorMessage = "El proveedor es obligatorio")]
        public int ProveedorId { get; set; }

        [ForeignKey("ProveedorId")]
        public Proveedor? Proveedor { get; set; }

        public string? ImagenUrl { get; set; }

        // Métodos de negocio
        public decimal CalcularValorInventario() => Precio * Stock;
        public bool TieneStock() => Stock > 0;
    }
}
