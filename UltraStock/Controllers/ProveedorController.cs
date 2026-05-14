using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using UltraStock.Data;
using UltraStock.Models;

namespace UltraStock.Controllers
{
    public class ProveedorController : Controller
    {
        private readonly AppDbContext _context;

        public ProveedorController(AppDbContext context)
        {
            _context = context;
        }

        // Solo roles con acceso de consulta o superior
        private bool TieneAcceso() => HttpContext.Session.GetString("Usuario") != null;

        private bool EsAdminOEncargado()
        {
            var rol = HttpContext.Session.GetString("Rol");
            return rol == "Administrador" || rol == "EncargadoAlmacen";
        }

        private bool EsAdmin() => HttpContext.Session.GetString("Rol") == "Administrador";

        public IActionResult Index()
        {
            if (!TieneAcceso()) return RedirectToAction("Index", "Login");
            var proveedores = _context.Proveedores.ToList();
            return View(proveedores);
        }

        public IActionResult Create()
        {
            if (!TieneAcceso()) return RedirectToAction("Index", "Login");
            if (!EsAdminOEncargado())
            {
                TempData["Error"] = "No tiene permisos para registrar proveedores.";
                return RedirectToAction("Index");
            }
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Proveedor proveedor)
        {
            if (!TieneAcceso()) return RedirectToAction("Index", "Login");
            if (!EsAdminOEncargado())
            {
                TempData["Error"] = "No tiene permisos para registrar proveedores.";
                return RedirectToAction("Index");
            }

            // Validar email único
            if (_context.Proveedores.Any(p => p.Email == proveedor.Email))
            {
                ModelState.AddModelError("Email", "Ya existe un proveedor con ese email.");
            }

            //temporal
            foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
            {
                Console.WriteLine(error.ErrorMessage);
            }

            if (ModelState.IsValid)
            {
                _context.Proveedores.Add(proveedor);
                _context.SaveChanges();
                TempData["Exito"] = "Proveedor registrado exitosamente.";
                return RedirectToAction("Index");
            }
            return View(proveedor);
        }

        public IActionResult Edit(int id)
        {
            if (!TieneAcceso()) return RedirectToAction("Index", "Login");
            if (!EsAdminOEncargado())
            {
                TempData["Error"] = "No tiene permisos para editar proveedores.";
                return RedirectToAction("Index");
            }
            var proveedor = _context.Proveedores.Find(id);
            if (proveedor == null) return NotFound();
            return View(proveedor);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Proveedor proveedor)
        {
            if (!TieneAcceso()) return RedirectToAction("Index", "Login");
            if (!EsAdminOEncargado())
            {
                TempData["Error"] = "No tiene permisos para editar proveedores.";
                return RedirectToAction("Index");
            }

            // Validar email único (excluyendo el actual)
            if (_context.Proveedores.Any(p => p.Email == proveedor.Email && p.Id != proveedor.Id))
            {
                ModelState.AddModelError("Email", "Ya existe un proveedor con ese email.");
            }

            if (ModelState.IsValid)
            {
                _context.Proveedores.Update(proveedor);
                _context.SaveChanges();
                TempData["Exito"] = "Proveedor actualizado exitosamente.";
                return RedirectToAction("Index");
            }
            return View(proveedor);
        }

        public IActionResult Delete(int id)
        {
            if (!TieneAcceso()) return RedirectToAction("Index", "Login");
            if (!EsAdmin())
            {
                TempData["Error"] = "Solo el Administrador puede eliminar proveedores.";
                return RedirectToAction("Index");
            }

            var proveedor = _context.Proveedores.Find(id);
            if (proveedor == null) return NotFound();

            // Validar que no tenga productos asociados
            if (_context.Productos.Any(p => p.ProveedorId == id))
            {
                TempData["Error"] = "No se puede eliminar: el proveedor tiene productos asociados.";
                return RedirectToAction("Index");
            }

            _context.Proveedores.Remove(proveedor);
            _context.SaveChanges();
            TempData["Exito"] = "Proveedor eliminado correctamente.";
            return RedirectToAction("Index");
        }
    }
}
