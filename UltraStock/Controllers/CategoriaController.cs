using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using UltraStock.Models;
using UltraStock.Data;

namespace UltraStock.Controllers
{
    public class CategoriaController : Controller
    {
        private readonly AppDbContext _context;

        public CategoriaController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            if (HttpContext.Session.GetString("Usuario") == null)
            {
                return RedirectToAction("Index", "Login");
            }

            var categorias = _context.Categorias.ToList();

            return View(categorias);
        }

        //Guardar categoria

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
        public IActionResult Create(Categoria categoria)
        {
            if (HttpContext.Session.GetString("Usuario") == null)
            {
                return RedirectToAction("Index", "Login");
            }

            if (!ModelState.IsValid)
                return View(categoria);

            _context.Categorias.Add(categoria);
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

            var categoria = _context.Categorias.Find(id);
            if (categoria == null)
                return NotFound();

            return View(categoria);
        }

        //Actualizar producto
        [HttpPost]
        public IActionResult Edit(Categoria categoria)
        {
            if (HttpContext.Session.GetString("Usuario") == null)
            {
                return RedirectToAction("Index", "Login");
            }

            if (!ModelState.IsValid)
                return View(categoria);

            _context.Categorias.Update(categoria);
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

            var rol = HttpContext.Session.GetString("Rol");//Solo admin puede eliminar
            if (rol != "Administrador")
            {
                return RedirectToAction("Index");
            }

            var categoria = _context.Categorias.Find(id);
            if (categoria == null)
                return NotFound();

            _context.Categorias.Remove(categoria);
            _context.SaveChanges();

            return RedirectToAction("Index");
        }


    }
}
