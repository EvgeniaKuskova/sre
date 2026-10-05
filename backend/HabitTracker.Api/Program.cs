using HabitTracker.Api.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Фактор 3: вся конфигурация, которая может меняться между средами,
// приходит из переменных окружения (они подключаются CreateBuilder автоматически).
builder.Configuration.AddEnvironmentVariables();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Факторы 3 и 4: пароли и строки подключения в appsettings.json не хранятся.
// Строка подключения задаётся только переменной окружения
// ConnectionStrings__DefaultConnection (см. .env.example).
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException(
        "Строка подключения к БД не задана. " +
        "Задайте переменную окружения ConnectionStrings__DefaultConnection (см. .env.example).");
}

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

var app = builder.Build();

// Фактор 12: схему БД приложение само НЕ создаёт.
// Схему накатывает одноразовый процесс миграций:
//   dotnet ef database update   (локально)
//   docker compose run --rm migrate   (в контейнерной среде)
// Рантайм-инстансы только читают/пишут, поэтому несколько реплик
// не конкурируют за создание схемы при старте (фактор 8).

// Фактор 7: адрес и порт привязки Kestrel берутся из переменных окружения
// (ASPNETCORE_URLS или PORT), в коде портов нет.
var port = builder.Configuration["PORT"];
if (!string.IsNullOrWhiteSpace(port))
{
    builder.WebHost.UseUrls($"http://+:{port}");
}

app.UseCors(x => x.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
app.MapControllers();

app.Run();
