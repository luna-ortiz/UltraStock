using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using UltraStock.Data;
using UltraStock.Models;

namespace UltraStock.Controllers
{
    public class ProductoController : Controller
    {
        private readonly AppDbContext _context;

        public ProductoController(AppDbContext context)
        {
            _context = context;
        }

        private bool TieneAcceso() => HttpContext.Session.GetString("Usuario") != null;

        private bool PuedeModificar()
        {
            var rol = HttpContext.Session.GetString("Rol");
            return rol == "Administrador" || rol == "EncargadoAlmacen";
        }

        private bool EsAdmin() => HttpContext.Session.GetString("Rol") == "Administrador";

        public IActionResult Index()
        {
            if (!TieneAcceso()) return RedirectToAction("Index", "Login");

            var productos = _context.Productos
                .Include(p => p.Categoria)
                .Include(p => p.Proveedor)
                .ToList();

            ViewBag.Rol = HttpContext.Session.GetString("Rol");
            return View(productos);
        }

        public IActionResult Create()
        {
            if (!TieneAcceso()) return RedirectToAction("Index", "Login");
            if (!PuedeModificar())
            {
                TempData["Error"] = "No tiene permisos para registrar productos.";
                return RedirectToAction("Index");
            }

            ViewBag.Categorias = _context.Categorias.ToList();
            ViewBag.Proveedores = _context.Proveedores.ToList();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Producto producto, IFormFile? imagen)
        {
            if (!TieneAcceso()) return RedirectToAction("Index", "Login");
            if (!PuedeModificar())
            {
                TempData["Error"] = "No tiene permisos para registrar productos.";
                return RedirectToAction("Index");
            }

            // Validar nombre único
            if (_context.Productos.Any(p => p.Nombre == producto.Nombre))
            {
                ModelState.AddModelError("Nombre", "Ya existe un producto con ese nombre.");
            }

            // Imagen opcional
            ModelState.Remove("ImagenUrl");

            if (ModelState.IsValid)
            {
                if (imagen != null && imagen.Length > 0)
                {
                    var extensiones = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
                    var ext = Path.GetExtension(imagen.FileName).ToLower();
                    if (!extensiones.Contains(ext))
                    {
                        ModelState.AddModelError("ImagenUrl", "Solo se permiten imágenes (jpg, png, gif, webp).");
                        ViewBag.Categorias = _context.Categorias.ToList();
                        ViewBag.Proveedores = _context.Proveedores.ToList();
                        return View(producto);
                    }

                    var carpeta = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images");
                    if (!Directory.Exists(carpeta)) Directory.CreateDirectory(carpeta);
                    var nombreArchivo = Guid.NewGuid().ToString() + ext;
                    var ruta = Path.Combine(carpeta, nombreArchivo);
                    using var stream = new FileStream(ruta, FileMode.Create);
                    imagen.CopyTo(stream);
                    producto.ImagenUrl = "/images/" + nombreArchivo;
                }

                _context.Productos.Add(producto);
                _context.SaveChanges();
                TempData["Exito"] = "Producto registrado exitosamente.";
                return RedirectToAction("Index");
            }

            ViewBag.Categorias = _context.Categorias.ToList();
            ViewBag.Proveedores = _context.Proveedores.ToList();
            return View(producto);
        }

        public IActionResult Edit(int id)
        {
            if (!TieneAcceso()) return RedirectToAction("Index", "Login");
            if (!PuedeModificar())
            {
                TempData["Error"] = "No tiene permisos para editar productos.";
                return RedirectToAction("Index");
            }

            var producto = _context.Productos.Find(id);
            if (producto == null) return NotFound();

            ViewBag.Categorias = _context.Categorias.ToList();
            ViewBag.Proveedores = _context.Proveedores.ToList();
            return View(producto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Producto producto, IFormFile? imagen)
        {
            if (!TieneAcceso()) return RedirectToAction("Index", "Login");
            if (!PuedeModificar())
            {
                TempData["Error"] = "No tiene permisos para editar productos.";
                return RedirectToAction("Index");
            }

            var productoDB = _context.Productos.Find(producto.Id);
            if (productoDB == null) return NotFound();

            // Validar nombre único (excluyendo el actual)
            if (_context.Productos.Any(p => p.Nombre == producto.Nombre && p.Id != producto.Id))
            {
                ModelState.AddModelError("Nombre", "Ya existe un producto con ese nombre.");
            }

            ModelState.Remove("ImagenUrl");

            if (ModelState.IsValid)
            {
                productoDB.Nombre = producto.Nombre;
                productoDB.Descripcion = producto.Descripcion;
                productoDB.Precio = producto.Precio;
                productoDB.Stock = producto.Stock;
                productoDB.CategoriaId = producto.CategoriaId;
                productoDB.ProveedorId = producto.ProveedorId;

                if (imagen != null && imagen.Length > 0)
                {
                    var extensiones = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
                    var ext = Path.GetExtension(imagen.FileName).ToLower();
                    if (!extensiones.Contains(ext))
                    {
                        ModelState.AddModelError("ImagenUrl", "Solo se permiten imágenes (jpg, png, gif, webp).");
                        ViewBag.Categorias = _context.Categorias.ToList();
                        ViewBag.Proveedores = _context.Proveedores.ToList();
                        return View(producto);
                    }

                    var carpeta = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images");
                    if (!Directory.Exists(carpeta)) Directory.CreateDirectory(carpeta);
                    var nombreArchivo = Guid.NewGuid().ToString() + ext;
                    var ruta = Path.Combine(carpeta, nombreArchivo);
                    using var stream = new FileStream(ruta, FileMode.Create);
                    imagen.CopyTo(stream);
                    productoDB.ImagenUrl = "/images/" + nombreArchivo;
                }

                _context.SaveChanges();
                TempData["Exito"] = "Producto actualizado exitosamente.";
                return RedirectToAction("Index");
            }

            ViewBag.Categorias = _context.Categorias.ToList();
            ViewBag.Proveedores = _context.Proveedores.ToList();
            return View(producto);
        }

        public IActionResult Delete(int id)
        {
            if (!TieneAcceso()) return RedirectToAction("Index", "Login");
            if (!EsAdmin())
            {
                TempData["Error"] = "Solo el Administrador puede eliminar productos.";
                return RedirectToAction("Index");
            }

            var producto = _context.Productos.Find(id);
            if (producto == null) return NotFound();

            // Validar que no tenga movimientos asociados
            if (_context.MovimientosInventario.Any(m => m.ProductoId == id))
            {
                TempData["Error"] = "No se puede eliminar: el producto tiene movimientos de inventario registrados.";
                return RedirectToAction("Index");
            }

            _context.Productos.Remove(producto);
            _context.SaveChanges();
            TempData["Exito"] = "Producto eliminado correctamente.";
            return RedirectToAction("Index");
        }
    }
}
