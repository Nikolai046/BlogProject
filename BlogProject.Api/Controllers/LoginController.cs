using BlogProject.Api.Services;
using BlogProject.Core.Models.RequestModels;
using BlogProject.Data.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace BlogProject.Api.Controllers;

[AllowAnonymous]
[Route("api/[controller]")]
[ApiController]
public class LoginController(UserManager<User> userManager, JwtService jwtService) : ControllerBase
{
    [HttpPost("login")]
    public async Task<ActionResult> Login(LoginRequest request)
    {
        var user = await userManager.FindByEmailAsync(request.Email);

        if (user == null || !await userManager.CheckPasswordAsync(user, request.Password))
        {
            return Unauthorized(new { ErrorMessage = "LoginController: Неверные учетные данные." });
        }

        try
        {
            var token = jwtService.GenerateToken(user.Id);
            Log.Information("LoginController: Пользователь {Email} авторизовался", request.Email);
            return Ok(new { Message = "Успешно вошли в систему.", AccessToken = token });
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Ошибка генерации JWT токена для пользователя {Email}", request.Email);
            return StatusCode(500, new { ErrorMessage = "Произошла ошибка при генерации токена." });
        }
    }
}