using System.ComponentModel.DataAnnotations;

namespace sistemaTickets.Models
{
    public class Categorias
    {
        [Key]
        [Display(Name = "Id categoria")]
        public int id_categorias { get; set; }

        [Display(Name = "Nombre")]
        public string nombre { get; set; }
    }
}
