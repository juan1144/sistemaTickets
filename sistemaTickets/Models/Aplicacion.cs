using System.ComponentModel.DataAnnotations;

namespace sistemaTickets.Models
{
    public class Aplicacion
    {
        [Key]
        [Display(Name = "Id")]
        public int id_aplicacion { get; set; }

        [Display(Name = "Id")]
        public string? nombre { get; set; }
    }
}
