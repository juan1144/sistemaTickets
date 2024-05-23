using Firebase.Auth;
using Firebase.Storage;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using sistemaTickets.Models;
using System.Linq;
using System.Net.Mail;
using System.Net.Sockets;
using static System.Collections.Specialized.BitVector32;
using static System.Net.Mime.MediaTypeNames;

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

            var categorias = _context.Categorias.ToList();

            ViewBag.Categorias = new SelectList(categorias, "id_categorias", "nombre");

            return View(usuario);
        }

        [HttpPost]
        public async Task<ActionResult> SubirTicket(int userId, string application, string subject, string description, string priority, int id_categorias, IFormFile attachment)
        {
            Stream archivoAsubir = attachment.OpenReadStream();

            //Configuramos conexión a Firebase
            string email = "juan.penate1@catolica.edu.sv";
            string clave = "contraseniaFirebase";
            string ruta = "sistematickets-31138.appspot.com";
            string api_key = "AIzaSyDSk8aWcHG9o9eS_5zVRK6lA1_vQkycEDM";

            var auth = new FirebaseAuthProvider(new FirebaseConfig(api_key));

            var autenticarFireBase = await auth.SignInWithEmailAndPasswordAsync(email, clave);

            var cancellation = new CancellationTokenSource();
            var tokenUser = autenticarFireBase.FirebaseToken;

            var tareaCargarArchivo = new FirebaseStorage(ruta, new FirebaseStorageOptions
            {
                AuthTokenAsyncFactory = () => Task.FromResult(tokenUser),
                ThrowOnCancel = true
            }).Child("Archivos").Child(attachment.FileName).PutAsync(archivoAsubir, cancellation.Token);

            var urlArchivoCargado = await tareaCargarArchivo;

            // Crear una instancia del modelo Tickets y asignar los valores recibidos del formulario
            var ticket = new Tickets
            {
                descripcion = description,
                id_categorias = id_categorias,
                creado_por = userId,
                asignado_a = null,
                asunto = subject,
                aplicacion = application,
                estado = "Pendiente",
                archivoAdj_id = urlArchivoCargado,
                fecha_creacion = DateTime.Now,
                fecha_actualizacion = null,
                prioridad = priority
                // Asignar los valores restantes...
            };

            // Insertar el ticket en la base de datos
            _context.Tickets.Add(ticket);
            await _context.SaveChangesAsync();

            // Redireccionar a la página de inicio u otra página según sea necesario
            return RedirectToAction("historialTickets", new { userId = userId });
        }

        public IActionResult historialTickets(int userId)
        {
            var usuario = _context.usuario.FirstOrDefault(u => u.id_usuario == userId);
            var ticketsAbiertos = _context.Tickets.Where(t => t.creado_por == userId && t.estado != "Cerrado").ToList();
            var ticketsCerrados = _context.Tickets.Where(t => t.creado_por == userId && t.estado == "Cerrado").ToList();

            ViewData["ticketsAbiertos"] = ticketsAbiertos;
            ViewData["ticketsCerrados"] = ticketsCerrados;

            return View(usuario);
        }

        public IActionResult gestionarUsuarios(int userId)
        {
            var usuario = _context.usuario.FirstOrDefault(u => u.id_usuario == userId);
            return View(usuario);
        }

        public IActionResult addUser(int userId)
        {
            var usuario = _context.usuario.FirstOrDefault(u => u.id_usuario == userId);
            var roles = _context.rol.ToList();

            ViewBag.roles = new SelectList(roles, "rolID", "Nombre");

            return View(usuario);
        }

        [HttpPost]
        public async Task<ActionResult> subirFormUserInt(int userId, int rolGeneral, string nombre, string apellido, string correo, string empresa, string telefono, string username, string password, IFormFile attachment)
        {
            if (_context.usuario.Any(u => u.user_nombre == username))
            {
                ModelState.AddModelError("user_nombre", "El nombre de usuario ya existe. Por favor, elige otro.");
                var roles = _context.rol.ToList();
                ViewBag.roles = new SelectList(roles, "rolID", "Nombre");
                return View("addUser", _context.usuario.FirstOrDefault(u => u.id_usuario == userId));
            }

            Stream archivoAsubir = attachment.OpenReadStream();

            //Configuramos conexión a Firebase
            string email = "juan.penate1@catolica.edu.sv";
            string clave = "contraseniaFirebase";
            string ruta = "sistematickets-31138.appspot.com";
            string api_key = "AIzaSyDSk8aWcHG9o9eS_5zVRK6lA1_vQkycEDM";

            var auth = new FirebaseAuthProvider(new FirebaseConfig(api_key));

            var autenticarFireBase = await auth.SignInWithEmailAndPasswordAsync(email, clave);

            var cancellation = new CancellationTokenSource();
            var tokenUser = autenticarFireBase.FirebaseToken;

            var tareaCargarArchivo = new FirebaseStorage(ruta, new FirebaseStorageOptions
            {
                AuthTokenAsyncFactory = () => Task.FromResult(tokenUser),
                ThrowOnCancel = true
            }).Child("Archivos").Child(attachment.FileName).PutAsync(archivoAsubir, cancellation.Token);

            var urlArchivoCargado = await tareaCargarArchivo;

            var nuevoUsuario = new usuario
            {
                nombre = nombre,
                apellido = apellido,
                correo = correo,
                empresa = empresa,
                telefono = telefono,
                user_nombre = username,
                contrasenia = password,
                imagen = urlArchivoCargado,
                rolID = rolGeneral
            };
            _context.usuario.Add(nuevoUsuario);
            await _context.SaveChangesAsync();

            return RedirectToAction("gestionarUsuarios", new { userId = userId });
        }

        [HttpPost]
        public async Task<ActionResult> subirFormUserExt(int userId, int rolGeneral, string nombre, string apellido, string correo, string username, string password, IFormFile attachment)
        {
            if (_context.usuario.Any(u => u.user_nombre == username))
            {
                ModelState.AddModelError("user_nombre", "El nombre de usuario ya existe. Por favor, elige otro.");
                var roles = _context.rol.ToList();
                ViewBag.roles = new SelectList(roles, "rolID", "Nombre");
                return View("addUser", _context.usuario.FirstOrDefault(u => u.id_usuario == userId));
            }

            Stream archivoAsubir = attachment.OpenReadStream();

            //Configuramos conexión a Firebase
            string email = "juan.penate1@catolica.edu.sv";
            string clave = "contraseniaFirebase";
            string ruta = "sistematickets-31138.appspot.com";
            string api_key = "AIzaSyDSk8aWcHG9o9eS_5zVRK6lA1_vQkycEDM";

            var auth = new FirebaseAuthProvider(new FirebaseConfig(api_key));

            var autenticarFireBase = await auth.SignInWithEmailAndPasswordAsync(email, clave);

            var cancellation = new CancellationTokenSource();
            var tokenUser = autenticarFireBase.FirebaseToken;

            var tareaCargarArchivo = new FirebaseStorage(ruta, new FirebaseStorageOptions
            {
                AuthTokenAsyncFactory = () => Task.FromResult(tokenUser),
                ThrowOnCancel = true
            }).Child("Archivos").Child(attachment.FileName).PutAsync(archivoAsubir, cancellation.Token);

            var urlArchivoCargado = await tareaCargarArchivo;

            var nuevoUsuario = new usuario
            {
                nombre = nombre,
                apellido = apellido,
                correo = correo,
                empresa = null,
                telefono = null,
                user_nombre = username,
                contrasenia = password,
                imagen = urlArchivoCargado,
                rolID = rolGeneral
            };
            _context.usuario.Add(nuevoUsuario);
            await _context.SaveChangesAsync();

            return RedirectToAction("gestionarUsuarios", new { userId = userId });
        }
    }
}


