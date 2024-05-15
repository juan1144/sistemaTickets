using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using sistemaTickets.Models;
using System.Linq;

namespace sistemaTickets.Controllers
{
    public class sistema_TicketsController : Controller
    {
        private readonly DB_TicketsDbContext _context;

        public sistema_TicketsController(DB_TicketsDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            // Verificar si el usuario ya está autenticado
            if (User.Identity.IsAuthenticated)
            {
                // Redirigir a la página de inicio
                return RedirectToAction("Inicio");
            }

            return View();
        }

        [HttpPost]
        public IActionResult Login(string user_nombre, string contrasenia)
        {
            // Buscar el usuario en la base de datos
            var usuario = _context.usuario.FirstOrDefault(u => u.user_nombre == user_nombre && u.contrasenia == contrasenia);

            if (usuario != null)
            {
                // Iniciar sesión y redirigir a la página de inicio
                // Aquí podrías utilizar ASP.NET Identity o implementar tu propia lógica de sesión
                // Por simplicidad, puedes almacenar el id del usuario en la sesión
                HttpContext.Session.SetInt32("UserId", usuario.id_usuario);
                return RedirectToAction("Inicio");
            }
            else
            {
                // Si el usuario no se encuentra, mostrar un mensaje de error
                ViewBag.ErrorMessage = "Usuario o contraseña incorrectos";
                return View("Index");
            }
        }

        public IActionResult Inicio()
        {
            // Obtener el id del usuario de la sesión
            var userId = HttpContext.Session.GetInt32("UserId");

            // Consultar la información del usuario desde la base de datos
            var usuario = _context.usuario.FirstOrDefault(u => u.id_usuario == userId);

            return View(usuario);
        }

        public IActionResult EnviarTicket(int userId)
        {
            var usuario = _context.usuario.FirstOrDefault(u => u.id_usuario == userId);

            var aplicaciones = _context.Aplicacion.ToList();
            var categorias = _context.Categorias.ToList();

            ViewBag.Aplicaciones = new SelectList(aplicaciones, "id_aplicacion", "nombre");
            ViewBag.Categorias = new SelectList(categorias, "id_categorias", "nombre");

            return View(usuario);
        }


        [HttpPost]
        public async Task<ActionResult> SubirTicket(int userId, string application, string subject, string description, string priority, int id_categorias, IFormFile attachment)
        {
            // Crear una instancia del modelo Tickets y asignar los valores recibidos del formulario
            var ticket = new Tickets
            {
                descripcion = description,
                // Asignar los valores restantes...
            };

            // Insertar el ticket en la base de datos
            _context.Tickets.Add(ticket);
            await _context.SaveChangesAsync();

            // Redireccionar a la página de inicio u otra página según sea necesario
            return RedirectToAction("Inicio");
        }

    }
}

