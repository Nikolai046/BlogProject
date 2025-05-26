using BlogProject.Core.CustomException;
using BlogProject.Data;
using BlogProject.Data.Entities;
using BlogProject.Data.Seeder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System.Net;

// Настройка Serilog до создания WebApplicationBuilder
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information() // Минимальный уровень логирования
    .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.Hosting.Lifetime", Serilog.Events.LogEventLevel.Information)
    .Enrich.FromLogContext()
    .WriteTo.Console() // вывод логов в консоль
    .WriteTo.File(
        path: "../Logs/log-.txt", // Путь к файлу лога
        rollingInterval: RollingInterval.Day, // Создавать новый файл каждый день
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}", // Формат записи
        retainedFileCountLimit: 7, // Хранить логи за последние 7 дней
        rollOnFileSizeLimit: true, // Создавать новый файл при достижении лимита размера
        fileSizeLimitBytes: 10_000_000) // Лимит размера файла (10MB)
    .CreateLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    // Используем Serilog для логирования ASP.NET Core
    builder.Host.UseSerilog();

    // Add services to the container.
    builder.Services.AddControllersWithViews();

    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection"))
        );

    builder.Services.AddIdentity<User, IdentityRole>(opts =>
    {
        opts.Password.RequiredLength = 5;
        opts.Password.RequireNonAlphanumeric = false;
        opts.Password.RequireLowercase = false;
        opts.Password.RequireUppercase = false;
        opts.Password.RequireDigit = false;
    }).AddEntityFrameworkStores<ApplicationDbContext>();

    builder.Services.AddScoped<TestDataGenerator>();

    builder.Services.AddAuthentication("CookieAuth")
        .AddCookie("CookieAuth", options =>
        {
            options.Cookie.Name = "AuthCookie";
            options.LoginPath = "/api/Auth/login";
            options.AccessDeniedPath = "/api/Auth/access-denied";
        });

    builder.Services.AddAuthorization(options =>
    {
        options.AddPolicy("RequireAdmin", policy =>
            policy.RequireRole("Admin"));
    });



    var app = builder.Build();

    // Configure the HTTP request pipeline.
    if (!app.Environment.IsDevelopment())
    {
        // Используем кастомный обработчик исключений
        app.UseExceptionHandler(errorApp =>
        {
            errorApp.Run(async context =>
            {
                var exceptionHandlerPathFeature = context.Features.Get<IExceptionHandlerPathFeature>();
                var exception = exceptionHandlerPathFeature?.Error;

                // Получаем ILogger для логирования
                var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();

                var (statusCode, message) = exception switch
                {
                    NotFoundException notFoundEx => (404, notFoundEx.Message),
                    ForbiddenException forbiddenEx => (403, forbiddenEx.Message),
                    DatabaseException dbEx => (500, dbEx.Message), // Обработка DatabaseException
                    ValidationException valEx => (400, valEx.Message), // Обработка ValidationException
                    AppException appEx => (appEx.StatusCode, appEx.Message),
                    _ => (500, "Произошла внутренняя ошибка сервера.")
                };

                // Логируем исключение
                logger.LogError("Ошибка: {ErrorMessage}, Код состояния: {StatusCode}, Путь: {Path}, Исключение: {ExceptionType}, Сообщение исключения: {ExceptionMessage}, StackTrace: {StackTrace}",
                                 message,
                                 statusCode,
                                 exceptionHandlerPathFeature?.Path,
                                 exception?.GetType().FullName,
                                 exception?.Message,
                                 exception?.StackTrace);


                context.Response.StatusCode = statusCode; // Устанавливаем код ответа перед редиректом
                var encodedMessage = WebUtility.UrlEncode(message);
                context.Response.Redirect($"/Error/Index?statusCode={statusCode}&message={encodedMessage}");
            });
        });
        app.UseHsts();
    }
    else
    {
        // В режиме разработки - DeveloperExceptionPage для более детальной информации
        app.UseDeveloperExceptionPage();
    }

    app.UseHttpsRedirection();
    app.UseStaticFiles();

    app.UseRouting();

    app.UseAuthentication();
    app.UseAuthorization();

    // Применение миграций и генерация тестовых данных
    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        db.Database.Migrate(); // Раскомментируйте, если миграции должны применяться при старте
        if (app.Environment.IsDevelopment())
        {
            var dataGen = scope.ServiceProvider.GetRequiredService<TestDataGenerator>();
            await dataGen.Generate();
        }
    }

    app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}");



    Log.Information("Приложение запускается...");
    app.Run();

}
catch (Exception ex)
{
    Log.Fatal(ex, "Приложение не смогло запуститься.");
}
finally
{
    Log.CloseAndFlush(); // Для записи всех логов перед завершением работы
}