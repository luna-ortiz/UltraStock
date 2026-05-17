using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Text.Json;
using UltraStock.Data;
using UltraStock.Models;

namespace UltraStock.Controllers
{
    public class SalidaController : Controller
    {
        private readonly AppDbContext _context;

        public SalidaController(AppDbContext context)
        {
            _context = context;
        }

        // Catálogo de productos para agregar a la salida (solo con stock)
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

        // Agregar producto al carrito de salida (igual que AgregarCarrito en ProductoController)
        public IActionResult AgregarCarrito(int id, int cantidad)
        {
            if (HttpContext.Session.GetString("Usuario") == null)
            {
                return RedirectToAction("Index", "Login");
            }

            var producto = _context.Productos.Find(id);

            // Validar stock (igual que en ProductoController)
            if (producto == null || producto.Stock == 0)
            {
                TempData["Error"] = "Producto sin existencias";
                return RedirectToAction("Index");
            }

            if (cantidad <= 0)
            {
                TempData["Error"] = "La cantidad debe ser mayor a 0";
                return RedirectToAction("Index");
            }

            if (cantidad > producto.Stock)
            {
                TempData["Error"] = "No hay disponibles tantas unidades";
                return RedirectToAction("Index");
            }

            var carritoJson = HttpContext.Session.GetString("CarritoSalida");

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
                "CarritoSalida",
                JsonSerializer.Serialize(carrito));

            TempData["Mensaje"] = "Producto agregado a la salida";

            return RedirectToAction("Index");
        }

        // Ver resumen del carrito de salida (igual que Carrito en ProductoController)
        public IActionResult Carrito()
        {
            if (HttpContext.Session.GetString("Usuario") == null)
            {
                return RedirectToAction("Index", "Login");
            }

            var carritoJson = HttpContext.Session.GetString("CarritoSalida");

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

        // Confirmar salida: resta stock y registra movimiento (igual que Comprar en ProductoController)
        [HttpPost]
        public IActionResult Confirmar(string observacion)
        {
            if (HttpContext.Session.GetString("Usuario") == null)
            {
                return RedirectToAction("Index", "Login");
            }

            var carritoJson = HttpContext.Session.GetString("CarritoSalida");

            if (carritoJson == null)
            {
                TempData["Error"] = "No hay productos en la salida.";
                return RedirectToAction("Index");
            }

            var carrito = JsonSerializer.Deserialize<List<CarritoItem>>(carritoJson) ?? new List<CarritoItem>();
            if (!carrito.Any())
            {
                TempData["Error"] = "No hay productos en la salida.";
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
                    if (producto.Stock < item.Cantidad)
                    {
                        TempData["Error"] = $"No hay stock suficiente para {producto.Nombre}.";
                        return RedirectToAction("Carrito");
                    }

                    producto.Stock -= item.Cantidad;

                    _context.MovimientosInventario.Add(new MovimientoInventario
                    {
                        ProductoId = item.ProductoId,
                        TipoMovimiento = "Salida",
                        Cantidad = item.Cantidad,
                        Fecha = DateTime.Now,
                        Observacion = observacion
                    });
                }
            }

            _context.SaveChanges();

            HttpContext.Session.Remove("CarritoSalida");

            TempData["Mensaje"] = "Salida registrada correctamente.";
            return RedirectToAction("Index", "Movimiento");
        }
    }
}
