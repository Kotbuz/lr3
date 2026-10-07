using Microsoft.EntityFrameworkCore;
using funny.Data;
using funny.Logic;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));


builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddTransient<IOldSexLogic, OldSexLogic>();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Funny API V1");
});

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllers();



using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<AppDbContext>();

        context.Database.EnsureDeleted();
        context.Database.EnsureCreated();

        if (!context.DigitalServices.Any())
        {
            var defaultServices = new[]
            {
                new funny.Models.DigitalService { Name = "Разработка Landing Page", Description = "Быстрый одностраничный сайт для конверсии", Price = 15000 },
                new funny.Models.DigitalService { Name = "Разработка Корпоративного сайта", Description = "Многостраничный сайт для вашей компании с админкой", Price = 45000 },
                new funny.Models.DigitalService { Name = "Настройка Яндекс.Директ", Description = "Контекстная реклама с гарантией целевых лидов", Price = 10000 },
                new funny.Models.DigitalService { Name = "SEO Оптимизация", Description = "Вывод вашего сайта в топ-10 поисковых систем", Price = 20000 },
                new funny.Models.DigitalService { Name = "Техническая поддержка 24/7", Description = "Мониторинг серверов и оперативное исправление багов", Price = 8000 }
            };

            context.DigitalServices.AddRange(defaultServices);
            context.SaveChanges();
        }
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Ошибка при автоматическом создании или заполнении БД.");
    }
}

app.Run();
