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
            if (HttpContext.Session.GetString("Usuario") == null)
            {
                return RedirectToAction("Index", "Login");
            }

            var usuarios = _context.Usuarios.ToList();

            return View(usuarios);
        }

        //Guardar usuario

        public IActionResult Create()
        {
            if (HttpContext.Session.GetString("Usuario") == null)
            {
                return RedirectToAction("Index", "Login");
            }

            return View();
        }

        [HttpPost]
        public IActionResult Create(Usuario usuario)
        {
            if (HttpContext.Session.GetString("Usuario") == null)
            {
                return RedirectToAction("Index", "Login");
            }

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
            if (HttpContext.Session.GetString("Usuario") == null)
            {
                return RedirectToAction("Index", "Login");
            }

            var usuario = _context.Usuarios.Find(id);
            if (usuario == null)
                return NotFound();

            ViewBag.Usuarios = _context.Usuarios.ToList();


            return View(usuario);
        }

        //Actualizar producto
        [HttpPost]
        public IActionResult Edit(Usuario usuario)
        {
            if (HttpContext.Session.GetString("Usuario") == null)
            {
                return RedirectToAction("Index", "Login");
            }

            ModelState.Remove(nameof(usuario.Clave));
            if (!ModelState.IsValid)
                return View(usuario);

            var usuarioBD = _context.Usuarios.Find(usuario.Id);
            if (usuarioBD == null)
                return NotFound();

            usuarioBD.Nombre = usuario.Nombre;
            usuarioBD.Correo = usuario.Correo;
            usuarioBD.Rol = usuario.Rol;
            usuarioBD.celular = usuario.celular;

            _context.SaveChanges();

            return RedirectToAction("Index");
        }

        //Eliminar usuario
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

            var usuario = _context.Usuarios.Find(id);
            if (usuario == null)
                return NotFound();

            _context.Usuarios.Remove(usuario);
            _context.SaveChanges();

            return RedirectToAction("Index");
        }


    }
}
