using Firebase.Auth;
using Firebase.Storage;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using sistemaTickets.Models;
using sistemaTickets.Services;
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

        private readonly IConfiguration _configuration;

        public sistema_TicketsController(DB_TicketsDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
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
            var ultimoticket=_context.Tickets.OrderByDescending(t => t.fecha_creacion).FirstOrDefault();

            // Crear instancia del servicio de correo y enviar un correo de prueba
            correo enviarCorreo = new correo(_configuration);
            string asunto = "[Ticket #"+ ultimoticket.id_ticket + "] Su solicitud ha sido registrada";
            string cuerpo = "Saludos, la solicitud " + ultimoticket.asunto + " ha sido registrada." + "\nTicket #:" + ultimoticket.id_ticket + "\nDescripión::" + ultimoticket.descripcion + "\nAplicación:" + ultimoticket.aplicacion + "\nPrioridad:" + ultimoticket.prioridad;

            enviarCorreo.enviar("juaanperezz518@gmail.com", asunto, cuerpo);

            // Redireccionar a la página de inicio u otra página según sea necesario
            return RedirectToAction("historialTickets", new { userId = userId });
        }

        public IActionResult historialTickets(int userId)
        {
            var usuario = _context.usuario.FirstOrDefault(u => u.id_usuario == userId);

            if (usuario == null)
            {
                return NotFound(); // Manejo de error si el usuario no es encontrado
            }

            List<Tickets> ticketsAbiertos;
            List<Tickets> ticketsCerrados;

            if (usuario.rolID == 2)
            {
                // Si el rol del usuario es 2, obtener todos los registros
                ticketsAbiertos = _context.Tickets.Where(t => t.estado != "Cerrado").ToList();
                ticketsCerrados = _context.Tickets.Where(t => t.estado == "Cerrado").ToList();
            }
            else
            {
                // Si el rol del usuario no es 2, obtener solo los registros creados por el usuario
                ticketsAbiertos = _context.Tickets.Where(t => t.creado_por == userId && t.estado != "Cerrado").ToList();
                ticketsCerrados = _context.Tickets.Where(t => t.creado_por == userId && t.estado == "Cerrado").ToList();
            }

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
                empresa = "",
                telefono = "",
                user_nombre = username,
                contrasenia = password,
                imagen = urlArchivoCargado,
                rolID = rolGeneral
            };
            _context.usuario.Add(nuevoUsuario);
            await _context.SaveChangesAsync();
            var ultimousuario = _context.usuario.OrderByDescending(t => t.id_usuario).FirstOrDefault();

            correo enviarCorreo = new correo(_configuration);
            string asunto = "Su usuario ha sido registrado";
            string cuerpo = "Saludos "+ultimousuario.nombre+ ",\n \nsu usuario es: " + ultimousuario.user_nombre + "\nSu contraseña es: "+ultimousuario.contrasenia;

            enviarCorreo.enviar("juaanperezz518@gmail.com", asunto, cuerpo);

            return RedirectToAction("gestionarUsuarios", new { userId = userId });
        }

        public IActionResult dashboard (int userId)
        {

            // Consultar la información del usuario desde la base de datos
            var usuario = _context.usuario.FirstOrDefault(u => u.id_usuario == userId);

            if (usuario == null)
            {
                // Manejar el caso en que el usuario no se encuentre
                return NotFound();
            }

            // Consultar todos los tickets
            var ticketsUsuario = _context.Tickets.ToList();

            // Calcular las cantidades de tickets por estado
            var ticketsAbiertosCount = ticketsUsuario.Count(t => t.estado == "Abierto");
            var ticketsCerradosCount = ticketsUsuario.Count(t => t.estado == "Cerrado");
            var ticketsPendienteCount = ticketsUsuario.Count(t => t.estado == "Pendiente");
            var ticketsAsignadosCount = ticketsUsuario.Count(t => t.asignado_a == userId);

            // Agregar la información del usuario y sus tickets a ViewBag
            ViewBag.Usuario = usuario;
            ViewBag.TicketsUsuario = ticketsUsuario;
            ViewBag.TicketsAbiertosCount = ticketsAbiertosCount;
            ViewBag.TicketsCerradosCount = ticketsCerradosCount;
            ViewBag.TicketsAsignadosCount = ticketsAsignadosCount;
            ViewBag.TicketsPendienteCount = ticketsPendienteCount;

            // Verificar los datos recuperados en consola para depuración
            Console.WriteLine("Usuario: " + usuario.nombre);
            Console.WriteLine("Total Tickets: " + ticketsUsuario.Count);

            return View("Dashboard", usuario);
        }

        public IActionResult viewUsers(int userId)
        {
            var usuario = _context.usuario.FirstOrDefault(u => u.id_usuario == userId);

            if (usuario == null)
            {
                return NotFound(); // Manejo de error si el usuario no es encontrado
            }

            List<usuario> listadoUsuarios = _context.usuario.ToList();

            ViewData["listadoUsuarios"] = listadoUsuarios;

            return View(usuario);
        }

        public IActionResult VerDetalleUsuario(int id)
        {
            var usuarioSeleccionado = _context.usuario.FirstOrDefault(u => u.id_usuario == id);

            if (usuarioSeleccionado == null)
            {
                return NotFound(); // Manejo de error si el usuario no es encontrado
            }

            var usuarioLogueadoId = HttpContext.Session.GetInt32("UserId");
            if (usuarioLogueadoId == null)
            {
                return RedirectToAction("Index", "Home"); // Redirigir al login si no hay un usuario logueado
            }

            var usuarioLogueado = _context.usuario.FirstOrDefault(u => u.id_usuario == usuarioLogueadoId);

            ViewBag.UserId = usuarioLogueadoId;
            ViewBag.RolID = usuarioLogueado.rolID;
            ViewBag.UserName = $"{usuarioLogueado.nombre} {usuarioLogueado.apellido}";
            ViewBag.UserImage = usuarioLogueado.imagen;

            return View(usuarioSeleccionado);
        }


        [HttpPost]
        public IActionResult EditarUsuario(usuario updatedUsuario)
        {
            var usuario = _context.usuario.FirstOrDefault(u => u.id_usuario == updatedUsuario.id_usuario);

            if (usuario == null)
            {
                return NotFound(); // Manejo de error si el usuario no es encontrado
            }

            if(usuario.rolID == 1)
            {
                usuario.empresa = "";
                usuario.telefono = "";
            }
            else
            {
                usuario.empresa = updatedUsuario.empresa;
                usuario.telefono = updatedUsuario.telefono;
            }

            usuario.nombre = updatedUsuario.nombre;
            usuario.apellido = updatedUsuario.apellido;
            usuario.correo = updatedUsuario.correo;
            usuario.user_nombre = updatedUsuario.user_nombre;
            usuario.contrasenia = updatedUsuario.contrasenia;
            usuario.rolID = updatedUsuario.rolID;

            _context.SaveChanges();

            return RedirectToAction("viewUsers", new { userId = HttpContext.Session.GetInt32("UserId") });
        }


        [HttpPost]
        public IActionResult EliminarUsuario(int id)
        {
            var usuario = _context.usuario.FirstOrDefault(u => u.id_usuario == id);

            if (usuario == null)
            {
                return NotFound(); // Manejo de error si el usuario no es encontrado
            }

            _context.usuario.Remove(usuario);
            _context.SaveChanges();

            return RedirectToAction("viewUsers", new { userId = HttpContext.Session.GetInt32("UserId") });
        }

        public IActionResult VerDetalleTicket(int id)
        {
            var ticket = _context.Tickets.FirstOrDefault(t => t.id_ticket == id);

            if (ticket == null)
            {
                return NotFound(); // Manejo de error si el ticket no es encontrado
            }

            var usuarioLogueadoId = HttpContext.Session.GetInt32("UserId");
            if (usuarioLogueadoId == null)
            {
                return RedirectToAction("Index", "Home"); // Redirigir al login si no hay un usuario logueado
            }

            var usuarioLogueado = _context.usuario.FirstOrDefault(u => u.id_usuario == usuarioLogueadoId);

            ViewBag.UserId = usuarioLogueadoId;
            ViewBag.RolID = usuarioLogueado.rolID;
            ViewBag.UserName = $"{usuarioLogueado.nombre} {usuarioLogueado.apellido}";
            ViewBag.UserImage = usuarioLogueado.imagen;

            var comentarios = _context.Comentarios
                .Where(c => c.id_ticket == id)
                .Join(_context.usuario,
                      comentario => comentario.id_usuario,
                      usuario => usuario.id_usuario,
                      (comentario, usuario) => new {
                          comentario.comentario,
                          comentario.fecha,
                          usuario.nombre,
                          usuario.apellido
                      })
                .ToList();
            ViewBag.Comentarios = comentarios;

            var usuarios = _context.usuario
                .Where(u => u.rolID == 2 || u.rolID == 3)
                .Select(u => new { u.id_usuario, u.nombre, u.apellido })
                .ToList();
            ViewBag.Usuarios = usuarios;

            return View(ticket);
        }

        [HttpPost]
        public IActionResult ActualizarEstado(int id_ticket, string estado)
        {
            var ticket = _context.Tickets.FirstOrDefault(t => t.id_ticket == id_ticket);
            if (ticket != null)
            {
                ticket.estado = estado;
                ticket.fecha_actualizacion = DateTime.Now;
                _context.SaveChanges();
            }
            return RedirectToAction("VerDetalleTicket", new { id = id_ticket });
        }

        [HttpPost]
        public IActionResult ActualizarAsignacion(int id_ticket, int asignado_a)
        {
            var ticket = _context.Tickets.FirstOrDefault(t => t.id_ticket == id_ticket);
            if (ticket != null)
            {
                ticket.asignado_a = asignado_a;
                ticket.fecha_actualizacion = DateTime.Now;
                _context.SaveChanges();
            }
            return RedirectToAction("VerDetalleTicket", new { id = id_ticket });
        }

        [HttpPost]
        public async Task<IActionResult> AgregarComentario(int id_ticket, string comentario)
        {
            var usuarioLogueadoId = HttpContext.Session.GetInt32("UserId");
            if (usuarioLogueadoId == null)
            {
                return RedirectToAction("Index", "Home"); // Redirigir al login si no hay un usuario logueado
            }

            var nuevoComentario = new Comentarios
            {
                id_ticket = id_ticket,
                comentario = comentario,
                fecha = DateTime.Now,
                id_usuario = usuarioLogueadoId
            };

            _context.Comentarios.Add(nuevoComentario);
            await _context.SaveChangesAsync();

            return RedirectToAction("VerDetalleTicket", new { id = id_ticket });
        }


    }
}



