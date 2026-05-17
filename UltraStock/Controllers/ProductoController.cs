using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Text.Json;
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

        public IActionResult Index()
        {
            if (HttpContext.Session.GetString("Usuario") == null)
            {
                return RedirectToAction("Index", "Login");
            }

            var productos = _context.Productos
                .Include(p => p.Categoria)
                .Include(p => p.Proveedor)
                .ToList();

            return View(productos);
        }

        //Guardar producto

        public IActionResult Create()
        {
            if (HttpContext.Session.GetString("Usuario") == null)
            {
                return RedirectToAction("Index", "Login");
            }

            ViewBag.Categorias = _context.Categorias.ToList();
            ViewBag.Proveedores = _context.Proveedores.ToList();
            return View();
        }

        [HttpPost]
        public IActionResult Create(Producto producto, IFormFile imagen)
        {
            if (HttpContext.Session.GetString("Usuario") == null)
            {
                return RedirectToAction("Index", "Login");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.Categorias = _context.Categorias.ToList();
                ViewBag.Proveedores = _context.Proveedores.ToList();
                return View(producto);
            }

            if (imagen != null && imagen.Length > 0)
            {
                var extension = Path.GetExtension(imagen.FileName).ToLowerInvariant();
                var extensionesPermitidas = new[] { ".jpg", ".jpeg", ".png", ".webp" };
                if (!extensionesPermitidas.Contains(extension))
                {
                    ModelState.AddModelError("ImagenUrl", "Solo se permiten imágenes JPG, PNG o WEBP.");
                    ViewBag.Categorias = _context.Categorias.ToList();
                    ViewBag.Proveedores = _context.Proveedores.ToList();
                    return View(producto);
                }

                var nombreArchivo = $"{Guid.NewGuid()}{extension}";
                var ruta = Path.Combine(Directory.GetCurrentDirectory(),
                    "wwwroot/images", nombreArchivo);

                using (var stream = new FileStream(ruta, FileMode.Create))
                {
                    imagen.CopyTo(stream);
                }

                producto.ImagenUrl = "/images/" + nombreArchivo;
            }

            _context.Productos.Add(producto);
            _context.SaveChanges();

            return RedirectToAction("Index");
        }

        //Formulario editar
        public IActionResult Edit(int id)
        {
            if (HttpContext.Session.GetString("Usuario") == null)
            {
                return RedirectToAction("Index", "Login");
            }

            var producto = _context.Productos.Find(id);
            if (producto == null)
                return NotFound();

            ViewBag.Categorias = _context.Categorias.ToList();
            ViewBag.Proveedores = _context.Proveedores.ToList();

            return View(producto);
        }

        [HttpPost]
        public IActionResult Edit(Producto producto, IFormFile imagen)
        {
            if (HttpContext.Session.GetString("Usuario") == null)
            {
                return RedirectToAction("Index", "Login");
            }

            var productoBD = _context.Productos.Find(producto.Id);
            if (productoBD == null)
                return NotFound();

            ModelState.Remove(nameof(producto.Categoria));
            ModelState.Remove(nameof(producto.Proveedor));
            ModelState.Remove(nameof(producto.ImagenUrl));
            if (!ModelState.IsValid)
            {
                ViewBag.Categorias = _context.Categorias.ToList();
                ViewBag.Proveedores = _context.Proveedores.ToList();
                return View(productoBD);
            }

            //Actualizar datos normales
            productoBD.Nombre = producto.Nombre;
            productoBD.Descripcion = producto.Descripcion;
            productoBD.Precio = producto.Precio;
            productoBD.Stock = producto.Stock;
            productoBD.CategoriaId = producto.CategoriaId;
            productoBD.ProveedorId = producto.ProveedorId;

            //Si sube nueva imagne
            if (imagen != null && imagen.Length > 0)
            {
                var extension = Path.GetExtension(imagen.FileName).ToLowerInvariant();
                var extensionesPermitidas = new[] { ".jpg", ".jpeg", ".png", ".webp" };
                if (!extensionesPermitidas.Contains(extension))
                {
                    ModelState.AddModelError("ImagenUrl", "Solo se permiten imágenes JPG, PNG o WEBP.");
                    ViewBag.Categorias = _context.Categorias.ToList();
                    ViewBag.Proveedores = _context.Proveedores.ToList();
                    return View(productoBD);
                }

                var carpeta = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images");

                if (!Directory.Exists(carpeta))
                {
                    Directory.CreateDirectory(carpeta);
                }

                var nombreArchivo = $"{Guid.NewGuid()}{extension}";
                var ruta = Path.Combine(carpeta, nombreArchivo);

                using (var stream = new FileStream(ruta, FileMode.Create))
                {
                    imagen.CopyTo(stream);
                }
                productoBD.ImagenUrl = "/images/" + nombreArchivo;
            }

            _context.SaveChanges();

            return RedirectToAction("Index");
        }

        //Eliminar producto
        public IActionResult Delete(int id)
        {
            if (HttpContext.Session.GetString("Usuario") == null)
            {
                return RedirectToAction("Index", "Login");
            }

            var rol = HttpContext.Session.GetString("Rol");

            //SOLO ADMIN PUEDE ELIMINAR
            if (rol != "Administrador")
            {
                return RedirectToAction("Index");
            }

            var producto = _context.Productos.Find(id);
            if (producto == null)
                return NotFound();

            _context.Productos.Remove(producto);
            _context.SaveChanges();

            return RedirectToAction("Index");
        }

    }
}
