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
    // ������� ������ ������������ ����������
    var builder = WebApplication.CreateBuilder(args);

    // ����������� Serilog ��� �����������, ��������� ������������ �� ����� ������������
    builder.Host.UseSerilog((context, config) =>
        config.ReadFrom.Configuration(context.Configuration));

    // ����������� ����������� � ���� ������ � �������������� SQLite
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseSqlite(
                builder.Configuration.GetConnectionString("DefaultConnection"),
                o => o.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery)
            )
            .UseLazyLoadingProxies()
    );

    // ���������� ���������
    builder.Services.AddFluentValidationAutoValidation();

    // ����������� ��������� ������������� �� ������������
    builder.Services.Configure<IdentityOptions>(
        builder.Configuration.GetSection("Identity"));

    // ��������� ������ ������������� � ���������, ��� �� ��������� ������������� ��������
    builder.Services.AddIdentity<User, IdentityRole>(options =>
        {
            options.SignIn.RequireConfirmedAccount = false;
        })
        .AddEntityFrameworkStores<ApplicationDbContext>()
        .AddDefaultTokenProviders();

    // ��������� ��������� ������������
    builder.Services.AddControllers();

    // ��������� ��������� ��� �������������� ��������� ������������ API
    builder.Services.AddEndpointsApiExplorer();

    // ��������� ��������� Swagger ��� ������������ API
    builder.Services.AddSwaggerGen();

    // ������ ����������
    var app = builder.Build();

    // ���� ���������� � ������ ����������, �������� Swagger ��� ������������ API
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

    // �������� ��������������� HTTP �� HTTPS
    app.UseHttpsRedirection();

    // �������� �����������
    app.UseAuthorization();

    // ����������� ������������� ��� ������������
    app.MapControllers();

    // �������� ���������� � ������� ����������
    Log.Information("\n\n\n���������� {AppName} �����������...", "BlogProject.Api");

    // ��������� ���������� ����������
    await app.RunAsync();
}
catch (Exception ex)
{
    // �������� ��������� ������, ���� ���������� �� ������ �����������
    Log.Fatal(ex, "\n\n\n���������� {AppName} �� ������ �����������.", "BlogProject.Api");
}
finally
{
    // ��������� � ������� ���
    Log.CloseAndFlush();
}