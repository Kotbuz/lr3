using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<ColaOrder> ColaOrders => Set<ColaOrder>();
        public DbSet<PizzaOrder> PizzaOrders => Set<PizzaOrder>();
        public DbSet<DigitalService> DigitalServices => Set<DigitalService>();
        public DbSet<Feedback> Feedbacks => Set<Feedback>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ColaOrder>().ToTable("cola_orders");
            modelBuilder.Entity<PizzaOrder>().ToTable("pizza_orders");
            modelBuilder.Entity<Feedback>().ToTable("feedbacks");

            var digital = modelBuilder.Entity<DigitalService>();
            digital.ToTable("digital_services");
            digital.Property(e => e.Price).HasPrecision(12, 2);
            // Стартовые данные для списка услуг
            digital.HasData(
                new DigitalService { Id = 1, Name = "Сопровождение решения 1С", Description = "Консультации и поддержка пользователей", Price = 5000 },
                new DigitalService { Id = 2, Name = "Доработка решения 1С", Description = "Изменение конфигурации под задачи бизнеса", Price = 12000 },
                new DigitalService { Id = 3, Name = "Обновление решения 1С", Description = "Установка актуальных релизов", Price = 3000 },
                new DigitalService { Id = 4, Name = "Внедрение 1С", Description = "Запуск системы под ключ", Price = 50000 }
            );
        }
    }
}
