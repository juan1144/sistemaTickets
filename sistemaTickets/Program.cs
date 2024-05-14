using Microsoft.EntityFrameworkCore;
using sistemaTickets.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<DB_TicketsDbContext>(opt =>
    opt.UseSqlServer(
            builder.Configuration.GetConnectionString("DB_TicketsDbConnection")
        )
);

builder.Services.AddSession(); // Agregar soporte de sesión

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.UseSession(); // Middleware para habilitar la sesión

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=sistema_Tickets}/{action=Index}/{id?}");

app.Run();

