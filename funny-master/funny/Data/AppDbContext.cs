using Microsoft.EntityFrameworkCore;
using funny.Models;

namespace funny.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<ConnectDialogColaVM> ColaOrders { get; set; }
        public DbSet<ConnectDialogPizzaVM> PizzaOrders { get; set; }
        public DbSet<ConnectDialogPolice> PoliceCalls { get; set; }
        public DbSet<DigitalService> DigitalServices { get; set; }
    }
}
