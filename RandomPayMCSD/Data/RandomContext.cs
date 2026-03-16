using Microsoft.EntityFrameworkCore;
using RandomPayMCSD.Models;

namespace RandomPayMCSD.Data
{
    public class RandomPayContext : DbContext
    {
        public RandomPayContext(DbContextOptions<RandomPayContext> options)
            : base(options) { }
        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Actividad> Actividades { get; set; }
        public DbSet<Participante> Participantes { get; set; }
        public DbSet<Gasto> Gastos { get; set; }
        public DbSet<Divisa> Divisas { get; set; }
        public DbSet<RepartoGasto> RepartosGasto { get; set; }
        public DbSet<ItemCompra> ItemsCompra { get; set; }
    }
    

}
