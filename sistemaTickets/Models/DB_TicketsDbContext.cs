using Microsoft.EntityFrameworkCore;

namespace sistemaTickets.Models
{
    public class DB_TicketsDbContext : DbContext
    {
        public DB_TicketsDbContext(DbContextOptions<DB_TicketsDbContext> options) : base(options) { }

        public DbSet<usuario> usuario { get; set; }
    }
}
