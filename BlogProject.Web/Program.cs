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

    // ��������� �������� � �������������� Serilog
    builder.Host.UseSerilog((context, config) =>
       config.ReadFrom.Configuration(context.Configuration));

    // ��������� ��������� ������������ � ������������� � ��������� �����
    builder.Services.AddControllersWithViews();

    // ��������� DataProtection
    builder.Services.AddDataProtection()
        .PersistKeysToFileSystem(new DirectoryInfo(@"./temp-keys")) // ������� ������������ ����, ��������� ��� ������
        .SetApplicationName("BlogProject");

    // ��������� �������� ���� ������ ApplicationDbContext � ��������� �����
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlite(
                builder.Configuration.GetConnectionString("DefaultConnection"),
                o => o.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery)
            )
            .UseLazyLoadingProxies()
);

    // ������������� ��������� �������������, ��������� ������ "Identity" �� ������������
    builder.Services.Configure<IdentityOptions>(
        builder.Configuration.GetSection("Identity"));

    // ��������� ������������� ��� ������������ � ����, ��������� Entity Framework ��� �������� ������
    builder.Services.AddIdentity<User, IdentityRole>(options =>
        {
            options.SignIn.RequireConfirmedAccount = false;
        })
        .AddEntityFrameworkStores<ApplicationDbContext>()
        .AddDefaultTokenProviders();

    // ��������� �������� ������
    builder.Services.AddScoped<TestDataGenerator>();

    // ��������� ����-�������������� ��� Identity
    builder.Services.ConfigureApplicationCookie(options =>
    {
        options.Cookie.Name = "AuthCookie";
        options.LoginPath = "/Login/Index";
        options.AccessDeniedPath = "/Error/Index=403&message=������+��������";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
        options.SlidingExpiration = true;
    });

    // ��������� ������ ��� ����������� � �������
    builder.Services.AddAuthorization();
    builder.Services.AddHttpContextAccessor();
    builder.Services.AddScoped<TestDataGenerator>();
    builder.Services.AddScoped<GetUserPermissions>();
    builder.Services.AddScoped<UserClaimsService>();

    // ������� ��������� ���������� � ������� �������
    var app = builder.Build();

    // ���������� ���������������� middleware ��� ��������� ����������.
    //app.UseCustomExceptionHandlingMiddleware();

    if (!app.Environment.IsDevelopment())
    {
        // �������� HSTS (HTTP Strict Transport Security) ��� ��������� ������������
        app.UseHsts();
    }
    else
    {
        // �������� �������� ���������� ��� �������������, ����� ������ ������ �� ����� ����������
        app.UseDeveloperExceptionPage();
    }

    // �������� ��������������� HTTP �� HTTPS
    app.UseHttpsRedirection();

    // ��������� ���������� ����������� ����������� �����
    app.UseStaticFiles();

    // ����������� ������������� ��� ����������
    app.UseRouting();

    // �������� �������������� �������������
    app.UseAuthentication();

    // �������� ����������� �������������
    app.UseAuthorization();

    // ���������� �������� � ��������� �������� ������
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

    // ��������� �������� ��� ������������
    app.MapControllerRoute(
         name: "default",
         pattern: "{controller=Home}/{action=Index}/{id?}");

    Log.Information("\n\n\n���������� �����������...");
    await app.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "\n\n\n���������� �� ������ �����������.");
}
finally
{
    // ��� ������ ���� ����� ����� ����������� ������
    Log.CloseAndFlush();
}