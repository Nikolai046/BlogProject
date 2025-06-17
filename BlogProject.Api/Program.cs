using BlogProject.Data;
using BlogProject.Data.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using FluentValidation;
using Serilog;
//using Scalar.AspNetCore.OpenApi;

try
{
    // Создаем объект конфигурации приложения
    var builder = WebApplication.CreateBuilder(args);

    // Настраиваем Serilog для логирования, используя конфигурацию из файла конфигурации
    builder.Host.UseSerilog((context, config) =>
        config.ReadFrom.Configuration(context.Configuration));

    // Настраиваем подключение к базе данных с использованием SQLite
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseSqlite(
                builder.Configuration.GetConnectionString("DefaultConnection"),
                o => o.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery)
            )
            .UseLazyLoadingProxies()
    );

    // Подключаем валидацию
    builder.Services.AddFluentValidationAutoValidation();

    // Настраиваем параметры идентификации из конфигурации
    builder.Services.Configure<IdentityOptions>(
        builder.Configuration.GetSection("Identity"));

    // Добавляем службы идентификации и указываем, что не требуется подтверждение аккаунта
    builder.Services.AddIdentity<User, IdentityRole>(options =>
        {
            options.SignIn.RequireConfirmedAccount = false;
        })
        .AddEntityFrameworkStores<ApplicationDbContext>()
        .AddDefaultTokenProviders();

    // Добавляем поддержку контроллеров
    builder.Services.AddControllers();

    // Добавляем поддержку для автоматической генерации документации API
    builder.Services.AddEndpointsApiExplorer();

    // Добавляем поддержку Swagger для документации API
    builder.Services.AddSwaggerGen();

    // Строим приложение
    var app = builder.Build();

    // Если приложение в режиме разработки, включаем Swagger для тестирования API
    if (app.Environment.IsDevelopment())
    {

        app.UseSwagger();
        app.UseSwaggerUI();

        app.MapScalarApiReference(options =>
        {
            options.WithOpenApiRoutePattern("/swagger/v1/swagger.json");
            options.WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient);
        });

    }

    // Включаем перенаправление HTTP на HTTPS
    app.UseHttpsRedirection();

    // Включаем авторизацию
    app.UseAuthorization();

    // Настраиваем маршрутизацию для контроллеров
    app.MapControllers();

    // Логируем информацию о запуске приложения
    Log.Information("\n\n\nПриложение {AppName} запускается...", "BlogProject.Api");

    // Запускаем приложение асинхронно
    await app.RunAsync();
}
catch (Exception ex)
{
    // Логируем фатальную ошибку, если приложение не смогло запуститься
    Log.Fatal(ex, "\n\n\nПриложение {AppName} не смогло запуститься.", "BlogProject.Api");
}
finally
{
    // Закрываем и очищаем лог
    Log.CloseAndFlush();
}