using System.ComponentModel.DataAnnotations;

namespace sistemaTickets.Models
{
    public class usuario
    {
        [Key]
        [Display(Name = "Id usuario")]
        public int id_usuario { get; set; }

        [Display(Name = "Nombre")]
        public string nombre { get; set; }

        [Display(Name = "Apellido")]
        public string apellido { get; set; }

        [Display(Name = "Correo")]
        public string correo { get; set; }

        [Display(Name = "Empresa")]
        public string? empresa { get; set; }

        [Display(Name = "Teléfono")]
        public string? telefono { get; set; }

        [Display(Name = "Usuario")]
        public string user_nombre { get; set; }

        [Display(Name = "Contraseña")]
        public string contrasenia { get; set; }

        [Display(Name = "Imagen")]
        public string imagen { get; set; }

        [Display(Name = "Id de Rol")]
        public int rolID { get; set; }
    }
}
