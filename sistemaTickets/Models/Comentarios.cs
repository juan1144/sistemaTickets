using System.ComponentModel.DataAnnotations;

namespace sistemaTickets.Models
{
    public class Comentarios
    {
        [Key]
        [Display(Name = "Id")]
        public int id_comentarios { get; set; }

        [Display(Name = "Comentario")]
        public string comentario { get; set; }

        [Display(Name = "Id de usuario")]
        public int? id_usuario { get; set; }

        [Display(Name = "Id ticket")]
        public int? id_ticket { get; set; }

        [Display(Name = "Fecha")]
        public DateTime? fecha { get; set; }
    }
}
