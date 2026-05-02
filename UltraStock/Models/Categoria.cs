using System.ComponentModel.DataAnnotations;
using UltraStock.Models;

namespace UltraStock.Models
{
    public class Categoria
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Nombre { get; set; }

        [Required]
        [StringLength(1000)]
        public string Descripcion { get; set; }


        [StringLength(100)]
        public string Estado { get; set; }
    }
}