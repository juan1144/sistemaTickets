using System.ComponentModel.DataAnnotations;

namespace sistemaTickets.Models
{
    public class Archivos_adjuntos
    {
        [Key]
        [Display(Name = "Id del archivo")]
        public int archivoAdj_id { get; set; }

        [Display(Name = "Id del ticket")]
        public int? id_ticket { get; set; }

        [Display(Name = "Nombre")]
        public string name { get; set; }

        [Display(Name = "Ruta")]
        public string path { get; set; }
    }
}
