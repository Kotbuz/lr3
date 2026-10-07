using Backend.Data;
using Backend.Logging;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using UsersProxy;

var builder = WebApplication.CreateBuilder(args);

// Логи: консоль + файл в папке logs
builder.Logging.AddProvider(new FileLoggerProvider(Path.Combine(builder.Environment.ContentRootPath, "logs")));

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Библиотека "прокси пользователи": запросы к смежной системе учёта пользователей
builder.Services.AddUsersProxy(builder.Configuration);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
    options.AddDefaultPolicy(policy => policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));

var app = builder.Build();

// Создаём таблицы в БД, если их ещё нет
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var creator = db.GetService<IRelationalDatabaseCreator>();
    if (!creator.Exists()) creator.Create();
    // Таблицы создаём только при первом запуске
    var tablesExist = db.Database
        .SqlQueryRaw<bool>("SELECT to_regclass('public.cola_orders') IS NOT NULL AS \"Value\"")
        .AsEnumerable()
        .First();
    if (!tablesExist) creator.CreateTables();

    // Таблица могла остаться от ЛР1, где у пиццы не было имени и телефона
    db.Database.ExecuteSqlRaw("ALTER TABLE pizza_orders ADD COLUMN IF NOT EXISTS \"Name\" text, ADD COLUMN IF NOT EXISTS \"Phone\" text");
}

app.UseSwagger();
app.UseSwaggerUI();

app.UseCors();
app.UseAuthorization();

app.MapControllers();

app.Run();
