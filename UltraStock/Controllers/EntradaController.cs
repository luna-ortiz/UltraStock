using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Text.Json;
using UltraStock.Data;
using UltraStock.Models;

namespace UltraStock.Controllers
{
    public class EntradaController : Controller
    {
        private readonly AppDbContext _context;

        public EntradaController(AppDbContext context)
        {
            _context = context;
        }

        // Catálogo de productos para agregar a la entrada
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

        // Agregar producto al carrito de entrada (igual que AgregarCarrito en ProductoController)
        public IActionResult AgregarCarrito(int id, int cantidad)
        {
            if (HttpContext.Session.GetString("Usuario") == null)
            {
                return RedirectToAction("Index", "Login");
            }

            var producto = _context.Productos.Find(id);

            if (producto == null)
            {
                TempData["Error"] = "Producto no encontrado";
                return RedirectToAction("Index");
            }

            if (cantidad <= 0)
            {
                TempData["Error"] = "La cantidad debe ser mayor a 0";
                return RedirectToAction("Index");
            }

            var carritoJson = HttpContext.Session.GetString("CarritoEntrada");

            List<CarritoItem> carrito;

            if (carritoJson == null)
            {
                carrito = new List<CarritoItem>();
            }
            else
            {
                carrito = JsonSerializer.Deserialize<List<CarritoItem>>(carritoJson) ?? new List<CarritoItem>();
            }

            var item = carrito.FirstOrDefault(p => p.ProductoId == id);

            if (item != null)
            {
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
                "CarritoEntrada",
                JsonSerializer.Serialize(carrito));

            TempData["Mensaje"] = "Producto agregado a la entrada";

            return RedirectToAction("Index");
        }

        // Ver resumen del carrito de entrada (igual que Carrito en ProductoController)
        public IActionResult Carrito()
        {
            if (HttpContext.Session.GetString("Usuario") == null)
            {
                return RedirectToAction("Index", "Login");
            }

            var carritoJson = HttpContext.Session.GetString("CarritoEntrada");

            List<CarritoItem> carrito;

            if (carritoJson == null)
                carrito = new List<CarritoItem>();
            else
                carrito = JsonSerializer.Deserialize<List<CarritoItem>>(carritoJson) ?? new List<CarritoItem>();

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

        // Confirmar entrada: suma stock y registra movimiento (igual que Comprar en ProductoController)
        [HttpPost]
        public IActionResult Confirmar(string observacion)
        {
            if (HttpContext.Session.GetString("Usuario") == null)
            {
                return RedirectToAction("Index", "Login");
            }

            var carritoJson = HttpContext.Session.GetString("CarritoEntrada");

            if (carritoJson == null)
            {
                TempData["Error"] = "No hay productos en la entrada.";
                return RedirectToAction("Index");
            }

            var carrito = JsonSerializer.Deserialize<List<CarritoItem>>(carritoJson) ?? new List<CarritoItem>();
            if (!carrito.Any())
            {
                TempData["Error"] = "No hay productos en la entrada.";
                return RedirectToAction("Index");
            }

            foreach (var item in carrito)
            {
                if (item.Cantidad <= 0)
                {
                    TempData["Error"] = "Todas las cantidades deben ser mayores a 0.";
                    return RedirectToAction("Carrito");
                }

                var producto = _context.Productos.Find(item.ProductoId);

                if (producto != null)
                {
                    // Suma al stock
                    producto.Stock += item.Cantidad;

                    // Registra el movimiento
                    _context.MovimientosInventario.Add(new MovimientoInventario
                    {
                        ProductoId = item.ProductoId,
                        TipoMovimiento = "Entrada",
                        Cantidad = item.Cantidad,
                        Fecha = DateTime.Now,
                        Observacion = observacion
                    });
                }
            }

            _context.SaveChanges();

            HttpContext.Session.Remove("CarritoEntrada");

            TempData["Mensaje"] = "Entrada registrada correctamente.";
            return RedirectToAction("Index", "Movimiento");
        }
    }
}
