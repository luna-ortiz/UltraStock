using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using UltraStock.Models;

namespace UltraStock.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<Producto> Productos { get; set; }
        public DbSet<Categoria> Categorias { get; set; }
        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Proveedor> Proveedores { get; set; }
        public DbSet<MovimientoInventario> MovimientosInventario { get; set; }

    }
}
