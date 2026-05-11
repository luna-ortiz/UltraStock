using System.ComponentModel.DataAnnotations;
using UltraStock.Models;

namespace UltraStock.Models
{
    public class Producto
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio")]
        [StringLength(100)]
        public string Nombre { get; set; }

        [Required]
        [Range(0, 1000000)]
        public double Precio { get; set; }

        [Range(0, 100)]
        public int Stock { get; set; }

        public int CategoriaId { get; set; }
        public Categoria Categoria { get; set; }

        public double CalcularValorInventario()
        {
            return Precio * Stock;
        }
        public bool TieneStock()
        {
            return Stock > 0;
        }
        [Required(ErrorMessage = "La imagen es obligatoria")]
        public string ImagenUrl { get; set; }
    }
}
