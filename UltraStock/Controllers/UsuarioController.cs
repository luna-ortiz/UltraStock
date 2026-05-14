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
        public UsuarioController(AppDbContext context) { _context = context; }

        private bool TieneAcceso() => HttpContext.Session.GetString("Usuario") != null;
        private bool EsAdmin() => HttpContext.Session.GetString("Rol") == "Administrador";

        public IActionResult Index()
        {
            if (!TieneAcceso()) return RedirectToAction("Index", "Login");
            if (!EsAdmin())
            {
                TempData["Error"] = "Solo el Administrador puede gestionar usuarios.";
                return RedirectToAction("Index", "Home");
            }
            var usuarios = _context.Usuarios.ToList();
            return View(usuarios);
        }
        //Guardar usuario
        public IActionResult Create()
        {
            if (!TieneAcceso()) return RedirectToAction("Index", "Login");
            if (!EsAdmin())
            {
                TempData["Error"] = "Solo el Administrador puede crear usuarios.";
                return RedirectToAction("Index", "Home");
            }
            ViewBag.Roles = new List<string> { "Administrador", "EncargadoAlmacen", "Supervisor", "Auxiliar", "Gerente" };
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Usuario usuario)
        {
            if (!TieneAcceso()) return RedirectToAction("Index", "Login");
            if (!EsAdmin())
            {
                TempData["Error"] = "Solo el Administrador puede crear usuarios.";
                return RedirectToAction("Index", "Home");
            }

            if (_context.Usuarios.Any(u => u.Correo == usuario.Correo))
            {
                ModelState.AddModelError("Correo", "Ya existe un usuario con ese correo.");
            }

            if (ModelState.IsValid)
            {
                //Convertir contraseña a Hash
                usuario.Clave = HashHelper.ObtenerHash(usuario.Clave);
                _context.Usuarios.Add(usuario);
                _context.SaveChanges();
                TempData["Exito"] = "Usuario registrado exitosamente.";
                return RedirectToAction("Index");
            }

            ViewBag.Roles = new List<string> { "Administrador", "EncargadoAlmacen", "Supervisor", "Auxiliar", "Gerente" };
            return View(usuario);
        }

        //formulario de edición
        public IActionResult Edit(int id)
        {
            if (!TieneAcceso()) return RedirectToAction("Index", "Login");
            if (!EsAdmin())
            {
                TempData["Error"] = "Solo el Administrador puede editar usuarios.";
                return RedirectToAction("Index", "Home");
            }

            var usuario = _context.Usuarios.Find(id);
            if (usuario == null) return NotFound();
            ViewBag.Roles = new List<string> { "Administrador", "EncargadoAlmacen", "Supervisor", "Auxiliar", "Gerente" };
            return View(usuario);
        }
        //actualizar usuario
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Usuario usuario)
        {
            if (!TieneAcceso()) return RedirectToAction("Index", "Login");
            if (!EsAdmin())
            {
                TempData["Error"] = "Solo el Administrador puede editar usuarios.";
                return RedirectToAction("Index", "Home");
            }

            if (_context.Usuarios.Any(u => u.Correo == usuario.Correo && u.Id != usuario.Id))
            {
                ModelState.AddModelError("Correo", "Ya existe un usuario con ese correo.");
            }

            // La clave en edición no se revalida si ya viene hasheada
            ModelState.Remove("Clave");

            if (ModelState.IsValid)
            {
                var usuarioDB = _context.Usuarios.Find(usuario.Id);
                if (usuarioDB == null) return NotFound();

                usuarioDB.Nombre = usuario.Nombre;
                usuarioDB.Correo = usuario.Correo;
                usuarioDB.Rol = usuario.Rol;
                usuarioDB.celular = usuario.celular;
                // Solo actualizar clave si se envió una nueva
                if (!string.IsNullOrWhiteSpace(usuario.Clave) && usuario.Clave.Length < 64)
                    usuarioDB.Clave = HashHelper.ObtenerHash(usuario.Clave);

                _context.SaveChanges();
                TempData["Exito"] = "Usuario actualizado exitosamente.";
                return RedirectToAction("Index");
            }

            ViewBag.Roles = new List<string> { "Administrador", "EncargadoAlmacen", "Supervisor", "Auxiliar", "Gerente" };
            return View(usuario);
        }

        public IActionResult Delete(int id)
        {
            if (!TieneAcceso()) return RedirectToAction("Index", "Login");
            if (!EsAdmin())
            {
                TempData["Error"] = "Solo el Administrador puede eliminar usuarios.";
                return RedirectToAction("Index");
            }

            // No puede eliminarse a sí mismo
            var usuarioActual = HttpContext.Session.GetString("Usuario");
            var usuario = _context.Usuarios.Find(id);
            if (usuario == null) return NotFound();

            if (usuario.Nombre == usuarioActual)
            {
                TempData["Error"] = "No puede eliminar su propio usuario.";
                return RedirectToAction("Index");
            }

            _context.Usuarios.Remove(usuario);
            _context.SaveChanges();
            TempData["Exito"] = "Usuario eliminado correctamente.";
            return RedirectToAction("Index");
        }
    }
}
