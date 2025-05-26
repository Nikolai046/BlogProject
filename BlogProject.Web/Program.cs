using BlogProject.Core.CustomException;
using BlogProject.Data;
using BlogProject.Data.Entities;
using BlogProject.Data.Seeder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System.Net;

// ��������� Serilog �� �������� WebApplicationBuilder
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information() // ����������� ������� �����������
    .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.Hosting.Lifetime", Serilog.Events.LogEventLevel.Information)
    .Enrich.FromLogContext()
    .WriteTo.Console() // ����� ����� � �������
    .WriteTo.File(
        path: "../Logs/log-.txt", // ���� � ����� ����
        rollingInterval: RollingInterval.Day, // ��������� ����� ���� ������ ����
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}", // ������ ������
        retainedFileCountLimit: 7, // ������� ���� �� ��������� 7 ����
        rollOnFileSizeLimit: true, // ��������� ����� ���� ��� ���������� ������ �������
        fileSizeLimitBytes: 10_000_000) // ����� ������� ����� (10MB)
    .CreateLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    // ���������� Serilog ��� ����������� ASP.NET Core
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
        // ���������� ��������� ���������� ����������
        app.UseExceptionHandler(errorApp =>
        {
            errorApp.Run(async context =>
            {
                var exceptionHandlerPathFeature = context.Features.Get<IExceptionHandlerPathFeature>();
                var exception = exceptionHandlerPathFeature?.Error;

                // �������� ILogger ��� �����������
                var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();

                var (statusCode, message) = exception switch
                {
                    NotFoundException notFoundEx => (404, notFoundEx.Message),
                    ForbiddenException forbiddenEx => (403, forbiddenEx.Message),
                    DatabaseException dbEx => (500, dbEx.Message), // ��������� DatabaseException
                    ValidationException valEx => (400, valEx.Message), // ��������� ValidationException
                    AppException appEx => (appEx.StatusCode, appEx.Message),
                    _ => (500, "��������� ���������� ������ �������.")
                };

                // �������� ����������
                logger.LogError("������: {ErrorMessage}, ��� ���������: {StatusCode}, ����: {Path}, ����������: {ExceptionType}, ��������� ����������: {ExceptionMessage}, StackTrace: {StackTrace}",
                                 message,
                                 statusCode,
                                 exceptionHandlerPathFeature?.Path,
                                 exception?.GetType().FullName,
                                 exception?.Message,
                                 exception?.StackTrace);


                context.Response.StatusCode = statusCode; // ������������� ��� ������ ����� ����������
                var encodedMessage = WebUtility.UrlEncode(message);
                context.Response.Redirect($"/Error/Index?statusCode={statusCode}&message={encodedMessage}");
            });
        });
        app.UseHsts();
    }
    else
    {
        // � ������ ���������� - DeveloperExceptionPage ��� ����� ��������� ����������
        app.UseDeveloperExceptionPage();
    }

    app.UseHttpsRedirection();
    app.UseStaticFiles();

    app.UseRouting();

    app.UseAuthentication();
    app.UseAuthorization();

    // ���������� �������� � ��������� �������� ������
    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        db.Database.Migrate(); // ����������������, ���� �������� ������ ����������� ��� ������
        if (app.Environment.IsDevelopment())
        {
            var dataGen = scope.ServiceProvider.GetRequiredService<TestDataGenerator>();
            await dataGen.Generate();
        }
    }

    app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}");



    Log.Information("���������� �����������...");
    app.Run();

}
catch (Exception ex)
{
    Log.Fatal(ex, "���������� �� ������ �����������.");
}
finally
{
    Log.CloseAndFlush(); // ��� ������ ���� ����� ����� ����������� ������
}