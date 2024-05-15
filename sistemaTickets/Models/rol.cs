using System.ComponentModel.DataAnnotations;

namespace sistemaTickets.Models
{
    public class rol
    {
        [Key]
        [Display(Name = "Id")]
        public int rolID { get; set; }

        [Display(Name = "Nombre")]
        public string Nombre { get; set; }

        [Display(Name = "Descripcion")]
        public string Descripcion { get; set; }
    }
}
