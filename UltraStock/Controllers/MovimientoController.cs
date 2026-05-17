using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using UltraStock.Models;
using UltraStock.Data;

namespace UltraStock.Controllers
{
    public class MovimientoController : Controller
    {
        private readonly AppDbContext _context;

        public MovimientoController(AppDbContext context)
        {
            _context = context;
        }

        // Lista de movimientos
        public IActionResult Index()
        {
            if (HttpContext.Session.GetString("Usuario") == null)
            {
                return RedirectToAction("Index", "Login");
            }

            var movimientos = _context.MovimientosInventario
                .Include(m => m.Producto)
                .OrderByDescending(m => m.Fecha)
                .ToList();

            return View(movimientos);
        }

        // Formulario editar
        public IActionResult Edit(int id)
        {
            if (HttpContext.Session.GetString("Usuario") == null)
            {
                return RedirectToAction("Index", "Login");
            }

            var movimiento = _context.MovimientosInventario.Find(id);
            if (movimiento == null)
                return NotFound();

            ViewBag.Productos = _context.Productos.ToList();

            return View(movimiento);
        }

        // Actualizar movimiento
        [HttpPost]
        public IActionResult Edit(MovimientoInventario movimiento)
        {
            if (HttpContext.Session.GetString("Usuario") == null)
            {
                return RedirectToAction("Index", "Login");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.Productos = _context.Productos.ToList();
                return View(movimiento);
            }

            var movimientoBD = _context.MovimientosInventario.Find(movimiento.Id);
            if (movimientoBD == null)
                return NotFound();

            var productoOriginal = _context.Productos.Find(movimientoBD.ProductoId);
            var productoNuevo = _context.Productos.Find(movimiento.ProductoId);
            if (productoOriginal == null || productoNuevo == null)
                return NotFound();

            // Revertir el efecto del movimiento original en el stock
            if (movimientoBD.TipoMovimiento == "Entrada")
                productoOriginal.Stock -= movimientoBD.Cantidad;
            else
                productoOriginal.Stock += movimientoBD.Cantidad;

            if (productoOriginal.Stock < 0)
            {
                ModelState.AddModelError("", "No se puede editar el movimiento porque dejaría stock negativo al revertir el registro original.");
                ViewBag.Productos = _context.Productos.ToList();
                return View(movimiento);
            }

            // Aplicar el nuevo movimiento al stock
            if (movimiento.TipoMovimiento == "Entrada")
            {
                productoNuevo.Stock += movimiento.Cantidad;
            }
            else
            {
                if (productoNuevo.Stock < movimiento.Cantidad)
                {
                    ModelState.AddModelError("", "No hay stock suficiente para registrar esta salida.");
                    ViewBag.Productos = _context.Productos.ToList();
                    return View(movimiento);
                }

                productoNuevo.Stock -= movimiento.Cantidad;
            }

            // Actualizar campos del movimiento
            movimientoBD.ProductoId = movimiento.ProductoId;
            movimientoBD.TipoMovimiento = movimiento.TipoMovimiento;
            movimientoBD.Cantidad = movimiento.Cantidad;
            movimientoBD.Fecha = movimiento.Fecha;
            movimientoBD.Observacion = movimiento.Observacion;

            _context.SaveChanges();

            return RedirectToAction("Index");
        }

        // Eliminar movimiento - solo Administrador
        public IActionResult Delete(int id)
        {
            if (HttpContext.Session.GetString("Usuario") == null)
            {
                return RedirectToAction("Index", "Login");
            }

            var rol = HttpContext.Session.GetString("Rol"); // Solo admin puede eliminar
            if (rol != "Administrador")
            {
                return RedirectToAction("Index");
            }

            var movimiento = _context.MovimientosInventario.Find(id);
            if (movimiento == null)
                return NotFound();

            var producto = _context.Productos.Find(movimiento.ProductoId);
            if (producto == null)
                return NotFound();

            // Revertir el stock al eliminar
            if (movimiento.TipoMovimiento == "Entrada")
            {
                if (producto.Stock < movimiento.Cantidad)
                {
                    TempData["Error"] = "No se puede eliminar esta entrada porque dejaría el stock en negativo.";
                    return RedirectToAction("Index");
                }
                producto.Stock -= movimiento.Cantidad;
            }
            else
            {
                producto.Stock += movimiento.Cantidad;
            }

            _context.MovimientosInventario.Remove(movimiento);
            _context.SaveChanges();

            return RedirectToAction("Index");
        }
    }
}
