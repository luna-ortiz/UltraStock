using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using UltraStock.Data;
using UltraStock.Models;

namespace UltraStock.Controllers
{
    public class MovimientoController : Controller
    {
        private readonly AppDbContext _context;

        public MovimientoController(AppDbContext context)
        {
            _context = context;
        }

        private bool TieneAcceso() => HttpContext.Session.GetString("Usuario") != null;

        private bool PuedeRegistrar()
        {
            var rol = HttpContext.Session.GetString("Rol");
            return rol == "Administrador" || rol == "EncargadoAlmacen" || rol == "Auxiliar";
        }

        private bool PuedeSoloVer()
        {
            var rol = HttpContext.Session.GetString("Rol");
            return rol == "Supervisor" || rol == "Gerente";
        }

        private bool EsAdmin() => HttpContext.Session.GetString("Rol") == "Administrador";

        public IActionResult Index()
        {
            if (!TieneAcceso()) return RedirectToAction("Index", "Login");

            var movimientos = _context.MovimientosInventario
                .Include(m => m.Producto)
                .OrderByDescending(m => m.Fecha)
                .ToList();

            return View(movimientos);
        }

        public IActionResult Create()
        {
            if (!TieneAcceso()) return RedirectToAction("Index", "Login");
            if (!PuedeRegistrar())
            {
                TempData["Error"] = "No tiene permisos para registrar movimientos.";
                return RedirectToAction("Index");
            }

            ViewBag.Productos = _context.Productos.ToList();
            ViewBag.TiposMovimiento = new List<string> { "Entrada", "Salida" };
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(MovimientoInventario movimiento)
        {
            if (!TieneAcceso()) return RedirectToAction("Index", "Login");
            if (!PuedeRegistrar())
            {
                TempData["Error"] = "No tiene permisos para registrar movimientos.";
                return RedirectToAction("Index");
            }

            var producto = _context.Productos.Find(movimiento.ProductoId);
            if (producto == null)
            {
                ModelState.AddModelError("ProductoId", "Producto no encontrado.");
            }
            else if (movimiento.TipoMovimiento == "Salida")
            {
                if (movimiento.Cantidad > producto.Stock)
                {
                    ModelState.AddModelError("Cantidad",
                        $"Stock insuficiente. Stock actual: {producto.Stock} unidades.");
                }
            }

            if (ModelState.IsValid)
            {
                movimiento.Fecha = DateTime.Now;

                // Actualizar stock del producto
                if (movimiento.TipoMovimiento == "Entrada")
                    producto!.Stock += movimiento.Cantidad;
                else
                    producto!.Stock -= movimiento.Cantidad;

                _context.MovimientosInventario.Add(movimiento);
                _context.SaveChanges();
                TempData["Exito"] = $"Movimiento registrado. Stock actualizado a {producto.Stock} unidades.";
                return RedirectToAction("Index");
            }

            ViewBag.Productos = _context.Productos.ToList();
            ViewBag.TiposMovimiento = new List<string> { "Entrada", "Salida" };
            return View(movimiento);
        }

        public IActionResult Details(int id)
        {
            if (!TieneAcceso()) return RedirectToAction("Index", "Login");

            var movimiento = _context.MovimientosInventario
                .Include(m => m.Producto)
                .FirstOrDefault(m => m.Id == id);

            if (movimiento == null) return NotFound();
            return View(movimiento);
        }

        // Solo Administrador puede eliminar movimientos
        public IActionResult Delete(int id)
        {
            if (!TieneAcceso()) return RedirectToAction("Index", "Login");
            if (!EsAdmin())
            {
                TempData["Error"] = "Solo el Administrador puede eliminar movimientos.";
                return RedirectToAction("Index");
            }

            var movimiento = _context.MovimientosInventario
                .Include(m => m.Producto)
                .FirstOrDefault(m => m.Id == id);

            if (movimiento == null) return NotFound();

            // Revertir stock
            var producto = movimiento.Producto;
            if (movimiento.TipoMovimiento == "Entrada")
                producto.Stock -= movimiento.Cantidad;
            else
                producto.Stock += movimiento.Cantidad;

            _context.MovimientosInventario.Remove(movimiento);
            _context.SaveChanges();
            TempData["Exito"] = "Movimiento eliminado y stock revertido.";
            return RedirectToAction("Index");
        }
    }
}
