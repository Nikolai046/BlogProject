using BlogProject.Data;
using BlogProject.Data.Entities;
using BlogProject.Data.Seeder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// �������� ILoggerFactory �� ���������� ��������� ������������
var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection"))
    .UseLoggerFactory(loggerFactory));

// ��������� Identity � ���������� ������������ � �������
builder.Services.AddIdentity<User, IdentityRole>(opts =>
{
    opts.Password.RequiredLength = 5;   // ����������� ����� ������
    opts.Password.RequireNonAlphanumeric = false;   // �� ��������� �� ���������-�������� ��������
    opts.Password.RequireLowercase = false; // �� ��������� �������� ����
    opts.Password.RequireUppercase = false; // �� ��������� ��������� ����
    opts.Password.RequireDigit = false; // �� ��������� ����

}).AddEntityFrameworkStores<ApplicationDbContext>();

// ����������� ���������� �������� ������
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

// ���������� �������� � ��������� �������� ������
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
