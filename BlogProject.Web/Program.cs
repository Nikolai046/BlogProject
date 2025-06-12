using BlogProject.Data;
using BlogProject.Data.Entities;
using BlogProject.Data.Seeder;
using BlogProject.Web.Middleware;
using BlogProject.Web.Services;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Serilog;

try
{
    var builder = WebApplication.CreateBuilder(args);

    // Настройки логгинга с использованием Serilog
    builder.Host.UseSerilog((context, config) =>
       config.ReadFrom.Configuration(context.Configuration));

    // Добавляем поддержку контроллеров и представлений в контейнер служб
    builder.Services.AddControllersWithViews();

    // Настройка DataProtection
    builder.Services.AddDataProtection()
        .PersistKeysToFileSystem(new DirectoryInfo(@"./temp-keys")) // Укажите существующий путь, доступный для записи
        .SetApplicationName("BlogProject");

    // Добавляем контекст базы данных ApplicationDbContext в контейнер служб
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlite(
                builder.Configuration.GetConnectionString("DefaultConnection"),
                o => o.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery)
            )
            .UseLazyLoadingProxies()
);

    // Конфигурируем параметры идентификации, используя секцию "Identity" из конфигурации
    builder.Services.Configure<IdentityOptions>(
        builder.Configuration.GetSection("Identity"));

    // Добавляем идентификацию для пользователя и роли, используя Entity Framework для хранения данных
    builder.Services.AddIdentity<User, IdentityRole>(options =>
        {
            options.SignIn.RequireConfirmedAccount = false;
        })
        .AddEntityFrameworkStores<ApplicationDbContext>()
        .AddDefaultTokenProviders();

    // Генерация тестовых данных
    builder.Services.AddScoped<TestDataGenerator>();

    // Настройка куки-аутентификации для Identity
    builder.Services.ConfigureApplicationCookie(options =>
    {
        options.Cookie.Name = "AuthCookie";
        options.LoginPath = "/Login/Index";
        options.AccessDeniedPath = "/Error/Index=403&message=Доступ+запрещен";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
        options.SlidingExpiration = true;
    });

    // Добавляем билдер для авторизации в сервисы
    builder.Services.AddAuthorization();
    builder.Services.AddHttpContextAccessor();
    builder.Services.AddScoped<TestDataGenerator>();
    builder.Services.AddScoped<GetUserPermissions>();
    builder.Services.AddScoped<UserClaimsService>();

    // Создаем экземпляр приложения с помощью билдера
    var app = builder.Build();

    // Используем пользовательский middleware для обработки исключений.
    //app.UseCustomExceptionHandlingMiddleware();

    if (!app.Environment.IsDevelopment())
    {
        // Включаем HSTS (HTTP Strict Transport Security) для повышения безопасности
        app.UseHsts();
    }
    else
    {
        // Включаем страницу исключений для разработчиков, чтобы видеть ошибки во время разработки
        app.UseDeveloperExceptionPage();
    }

    // Включает перенаправление HTTP на HTTPS
    app.UseHttpsRedirection();

    // Позволяет приложению обслуживать статические файлы
    app.UseStaticFiles();

    // Настраивает маршрутизацию для приложения
    app.UseRouting();

    // Включает аутентификацию пользователей
    app.UseAuthentication();

    // Включает авторизацию пользователей
    app.UseAuthorization();

    // Применение миграций и генерация тестовых данных
    using (var scope = app.Services.CreateScope())
    {
        if (app.Environment.IsDevelopment())
        {
            //var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            //db.Database.Migrate();
            //var dataGen = scope.ServiceProvider.GetRequiredService<TestDataGenerator>();
            //await dataGen.Generate();
        }
    }

    // Настройка маршрута для контроллеров
    app.MapControllerRoute(
         name: "default",
         pattern: "{controller=Home}/{action=Index}/{id?}");

    Log.Information("\n\n\nПриложение запускается...");
    await app.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "\n\n\nПриложение не смогло запуститься.");
}
finally
{
    // Для записи всех логов перед завершением работы
    Log.CloseAndFlush();
}