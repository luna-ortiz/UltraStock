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

        //lista de produtos
        public IActionResult Index()
        {
            if (HttpContext.Session.GetString("Usuario") == null) //validar que no ingrese sin usuario (redirecciona a login)
            {
                return RedirectToAction("Index", "Login");
            }

            var productos = _context.Productos
                .Include(p => p.Categoria)
                .ToList();

            return View(productos);
        }

        //formulario crear
        public IActionResult Create()
        {
            if (HttpContext.Session.GetString("Usuario") == null) //validar que no ingrese sin usuario (redirecciona a login)
            {
                return RedirectToAction("Index", "Login");
            }

            ViewBag.Categorias = _context.Categorias.ToList();
            return View();
        }

        //guardar producto
        [HttpPost]
        public IActionResult Create(Producto producto)
        {
            if (HttpContext.Session.GetString("Usuario") == null) //validar que no ingrese sin usuario (redirecciona a login)
            {
                return RedirectToAction("Index", "Login");
            }

            _context.Productos.Add(producto);
            _context.SaveChanges();

            return RedirectToAction("Index");
        }

        //formulario editar
        public IActionResult Edit(int id)
        {
            if (HttpContext.Session.GetString("Usuario") == null) //validar que no ingrese sin usuario (redirecciona a login)
            {
                return RedirectToAction("Index", "Login");
            }

            var producto = _context.Productos.Find(id);
            ViewBag.Categorias = _context.Categorias.ToList();

            return View(producto);
        }

        //Actualizar producto
        [HttpPost]
        public IActionResult Edit(Producto producto)
        {
            if (HttpContext.Session.GetString("Usuario") == null) //validar que no ingrese sin usuario (redirecciona a login)
            {
                return RedirectToAction("Index", "Login");
            }

            _context.Productos.Update(producto);
            _context.SaveChanges();

            return RedirectToAction("Index");
        }

        //Eliminar producto
        public IActionResult Delete(int id)
        {
            if (HttpContext.Session.GetString("Usuario") == null) //validar que no ingrese sin usuario (redirecciona a login)
            {
                return RedirectToAction("Index", "Login");
            }

            var rol = HttpContext.Session.GetString("Rol");//Solo admin puede eliminar
            if (rol != "admin")
            {
                return RedirectToAction("Index");
            }

            var producto = _context.Productos.Find(id);

            _context.Productos.Remove(producto);
            _context.SaveChanges();

            return RedirectToAction("Index");
        }
    }
}
