using Microsoft.AspNetCore.Mvc;
using System.Linq;
using UltraStock.Data;
using UltraStock.Models;
using UltraStock.Helpers;

namespace UltraStock.Controllers
{
    public class LoginController : Controller
    {
        private readonly AppDbContext _context;
        public LoginController(AppDbContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            return View();
        }
        [HttpPost]
        public IActionResult Index(string correo, string clave) //recibe correo y clave
        {
            string claveHash = HashHelper.ObtenerHash(clave);

            var usuario = _context.Usuarios //conecta con bd a la tabla de usuarios
                .FirstOrDefault(u => u.Correo == correo && u.Clave == claveHash); //envia y evalua con los datos de la tabla
            if (usuario != null)
            {
                HttpContext.Session.SetString("Usuario", usuario.Nombre);
                HttpContext.Session.SetString("Rol", usuario.Rol);

                return RedirectToAction("Index", "Home");
            }
            ViewBag.Error = "Credenciales incorrectas";
            return View();
        }
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index");
        }
    }
}
