using Microsoft.EntityFrameworkCore;

namespace sistemaTickets.Models
{
    public class DB_TicketsDbContext : DbContext
    {
        public DB_TicketsDbContext(DbContextOptions<DB_TicketsDbContext> options) : base(options) { }

        public DbSet<usuario> usuario { get; set; }
        public DbSet<rol> rol { get; set; }
        public DbSet<Categorias> Categorias { get; set; }
        public DbSet<Tickets> Tickets { get; set; }
        public DbSet<Archivos_adjuntos> Archivos_adjuntos { get; set; }
        public DbSet<Comentarios> Comentarios { get; set; }
    }
}
