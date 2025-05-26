using BlogProject.Data;
using BlogProject.Data.Entities;
using BlogProject.Data.Seeder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Получаем ILoggerFactory из контейнера внедрения зависимостей
var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection"))
    .UseLoggerFactory(loggerFactory));

// Настройка Identity с усиленными требованиями к паролям
builder.Services.AddIdentity<User, IdentityRole>(opts =>
{
    opts.Password.RequiredLength = 5;   // Минимальная длина пароля
    opts.Password.RequireNonAlphanumeric = false;   // Не требовать не алфавитно-цифровых символов
    opts.Password.RequireLowercase = false; // Не требовать строчных букв
    opts.Password.RequireUppercase = false; // Не требовать заглавных букв
    opts.Password.RequireDigit = false; // Не требовать цифр

}).AddEntityFrameworkStores<ApplicationDbContext>();

// Регистрация генератора тестовых данных
builder.Services.AddScoped<TestDataGenerator>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

// Применение миграций и генерация тестовых данных
using (var scope = app.Services.CreateScope())
{
    //var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    //  db.Database.Migrate();
    if (app.Environment.IsDevelopment())
    {
        var dataGen = scope.ServiceProvider.GetRequiredService<TestDataGenerator>();
        await dataGen.Generate();
    }
}

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
