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
            return View();
        }

        [HttpPost]
        public IActionResult Create(Producto producto, IFormFile imagen)
        {
            if (HttpContext.Session.GetString("Usuario") == null)
            {
                return RedirectToAction("Index", "Login");
            }

            if (imagen != null)
            {
                var ruta = Path.Combine(Directory.GetCurrentDirectory(),
                    "wwwroot/images", imagen.FileName);

                using (var stream = new FileStream(ruta, FileMode.Create))
                {
                    imagen.CopyTo(stream);
                }

                producto.ImagenUrl = "/images/" + imagen.FileName;
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
            ViewBag.Categorias = _context.Categorias.ToList();

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
            //Actualizar datos normales
            productoBD.Nombre = producto.Nombre;
            productoBD.Precio = producto.Precio;
            productoBD.Stock = producto.Stock;
            productoBD.CategoriaId = producto.CategoriaId;

            //Si sube nueva imagne
            if (imagen != null)
            {
                var carpeta = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images");

                if (!Directory.Exists(carpeta))
                {
                    Directory.CreateDirectory(carpeta);
                }

                var ruta = Path.Combine(carpeta, imagen.FileName);

                using (var stream = new FileStream(ruta, FileMode.Create))
                {
                    imagen.CopyTo(stream);
                }
                productoBD.ImagenUrl = "/images/" + imagen.FileName;
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
            if (rol != "admin")
            {
                return RedirectToAction("Index");
            }

            var producto = _context.Productos.Find(id);

            _context.Productos.Remove(producto);
            _context.SaveChanges();

            return RedirectToAction("Index");
        }

        public IActionResult AgregarCarrito(int id, int cantidad)
        {
            var producto = _context.Productos.Find(id);
            // Validar Stock
            if (producto == null || producto.Stock == 0)
            {
                TempData["Error"] = "Producto sin existencias";
                return RedirectToAction("Index");
            }

            // Validar cantidad
            if (cantidad > producto.Stock)
            {
                TempData["Error"] = "No hay disponibles tantas unidades";
                return RedirectToAction("Index");
            }

            var carritoJson = HttpContext.Session.GetString("Carrito");

            List<CarritoItem> carrito;

            if (carritoJson == null)
            {
                carrito = new List<CarritoItem>();
            }
            else
            {
                carrito = System.Text.Json.JsonSerializer
                    .Deserialize<List<CarritoItem>>(carritoJson);
            }

            var item = carrito.FirstOrDefault(p => p.ProductoId == id);

            if (item != null)
            {
                if ((item.Cantidad + cantidad) > producto.Stock)
                {
                    TempData["Error"] = "No hay suficientes unidades disponibles";
                    return RedirectToAction("Index");
                }

                item.Cantidad += cantidad;
            }
            else
            {
                carrito.Add(new CarritoItem
                {
                    ProductoId = id,
                    Cantidad = cantidad
                });
            }

            HttpContext.Session.SetString(
                "Carrito",
                System.Text.Json.JsonSerializer.Serialize(carrito));

            // Mensaje éxito
            TempData["Mensaje"] = "Producto agregado al carrito";

            return RedirectToAction("Index");
        }

        public IActionResult Carrito()
        {
            var carritoJson = HttpContext.Session.GetString("Carrito");
            List<CarritoItem> carrito;

            if (carritoJson == null)
                carrito = new List<CarritoItem>();
            else
                carrito = JsonSerializer.Deserialize<List<CarritoItem>>(carritoJson);

            var productos = new List<(Producto producto, int cantidad)>();

            foreach (var item in carrito)
            {
                var producto = _context.Productos.Find(item.ProductoId);

                if (producto != null)
                {
                    productos.Add((producto, item.Cantidad));
                }
            }

            return View(productos);
        }

        public IActionResult Comprar()
        {
            var carritoJson = HttpContext.Session.GetString("carrito");

            if (carritoJson == null)
                return RedirectToAction("Index");

            var carrito = JsonSerializer.Deserialize<List<CarritoItem>>(carritoJson);

            foreach (var item in carrito)
            {
                var producto = _context.Productos.Find(item.ProductoId);

                if (producto != null)
                {
                    if (producto.Stock >= item.Cantidad)
                    {
                        producto.Stock -= item.Cantidad;
                    }
                }
            }

            _context.SaveChanges();

            HttpContext.Session.Remove("Carrito");

            return RedirectToAction("Index");
        }

    }
}
