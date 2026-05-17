using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using UltraStock.Models;
using UltraStock.Data;

namespace UltraStock.Controllers
{
    public class ProveedorController : Controller
    {
        private readonly AppDbContext _context;

        public ProveedorController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            if (HttpContext.Session.GetString("Usuario") == null)
            {
                return RedirectToAction("Index", "Login");
            }

            var proveedores = _context.Proveedores.ToList();

            return View(proveedores);
        }

        //Guardar proveedor

        public IActionResult Create()
        {
            if (HttpContext.Session.GetString("Usuario") == null)
            {
                return RedirectToAction("Index", "Login");
            }

            ViewBag.Proveedores = _context.Proveedores.ToList();
            return View();
        }

        [HttpPost]
        public IActionResult Create(Proveedor proveedor)
        {
            if (HttpContext.Session.GetString("Usuario") == null)
            {
                return RedirectToAction("Index", "Login");
            }

            _context.Proveedores.Add(proveedor);
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

            var proveedor = _context.Proveedores.Find(id);

            return View(proveedor);
        }

        //Actualizar proveedor
        [HttpPost]
        public IActionResult Edit(Proveedor proveedor)
        {
            if (HttpContext.Session.GetString("Usuario") == null)
            {
                return RedirectToAction("Index", "Login");
            }

            _context.Proveedores.Update(proveedor);
            _context.SaveChanges();

            return RedirectToAction("Index");
        }

        //Eliminar proveedor
        public IActionResult Delete(int id)
        {
            if (HttpContext.Session.GetString("Usuario") == null)
            {
                return RedirectToAction("Index", "Login");
            }

            var rol = HttpContext.Session.GetString("Rol");//Solo admin puede eliminar
            if (rol != "admin")
            {
                return RedirectToAction("Index");
            }

            var proveedor = _context.Proveedores.Find(id);

            _context.Proveedores.Remove(proveedor);
            _context.SaveChanges();

            return RedirectToAction("Index");
        }


    }
}
