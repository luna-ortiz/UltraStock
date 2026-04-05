using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using UltraStock.Data;
using UltraStock.Models;

namespace UltraStock.Controllers
{
    public class UsuarioController : Controller
    {
        private readonly AppDbContext _context;

        public UsuarioController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var usuarios = _context.Usuarios.ToList();

            return View(usuarios);
        }

        //Guardar usuario

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(Usuario usuario)
        {
            _context.Usuarios.Add(usuario);
            _context.SaveChanges();

            return RedirectToAction("Index");
        }

        //Formulario editar
        public IActionResult Edit(int id)
        {
            var usuario = _context.Usuarios.Find(id);

            return View(usuario);
        }

        //Eliminar usuario
        public IActionResult Delete(int id)
        {
            var usuario = _context.Usuarios.Find(id);

            _context.Usuarios.Remove(usuario);
            _context.SaveChanges();

            return RedirectToAction("Index");
        }


    }
}
