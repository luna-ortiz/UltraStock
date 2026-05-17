using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using UltraStock.Data;
using UltraStock.Helpers;
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


            if (ModelState.IsValid)
            {
                //Convertir contraseña a Hash
                usuario.Clave = HashHelper.ObtenerHash(usuario.Clave);
                _context.Usuarios.Add(usuario);
                _context.SaveChanges();

                return RedirectToAction("Index");
            }

            return View(usuario);
        }

        //Formulario editar
        public IActionResult Edit(int id)
        {


            var usuario = _context.Usuarios.Find(id);
            ViewBag.Usuarios = _context.Usuarios.ToList();


            return View(usuario);
        }

        //Actualizar producto
        [HttpPost]
        public IActionResult Edit(Usuario usuario)
        {


            _context.Usuarios.Update(usuario);
            _context.SaveChanges();

            return RedirectToAction("Index");
        }

        //Eliminar usuario
        public IActionResult Delete(int id)
        {


            var rol = HttpContext.Session.GetString("Rol");//Solo admin puede eliminar
            if (rol != "admin")
            {
                return RedirectToAction("Index");
            }

            var usuario = _context.Usuarios.Find(id);

            _context.Usuarios.Remove(usuario);
            _context.SaveChanges();

            return RedirectToAction("Index");
        }


    }
}
