using System.ComponentModel.DataAnnotations;

namespace sistemaTickets.Models
{
    public class Tickets
    {
        [Key]
        [Display(Name = "Id")]
        public int id_ticket { get; set; }

        [Display(Name = "Descripcion")]
        public string descripcion { get; set; }

        [Display(Name = "Id de categorias")]
        public int? id_categorias { get; set; }

        [Display(Name = "Creado por")]
        public int? creado_por { get; set; }

        [Display(Name = "Asignado a")]
        public int? asignado_a { get; set; }

        [Display(Name = "Asunto")]
        public string asunto { get; set; }

        [Display(Name = "Aplicacion")]
        public string aplicacion { get; set; }

        [Display(Name = "Estado")]
        public string estado { get; set; }

        [Display(Name = "Id del archivo Adj")]
        public string archivoAdj_id { get; set; }

        [Display(Name = "Fecha de creacion")]
        public DateTime fecha_creacion { get; set; }

        [Display(Name = "Fecha de actualizacion")]
        public DateTime? fecha_actualizacion { get; set; }

        [Display(Name = "Prioridad")]
        public string prioridad { get; set; }

        
         
    }
}
