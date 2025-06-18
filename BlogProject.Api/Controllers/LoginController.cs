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
public class LoginController(SignInManager<User> signInManager, JwtService jwtService) : ControllerBase
{
    [HttpPost("login")]
    public async Task<ActionResult> Login(LoginRequest request)
    {
        var result = await signInManager.PasswordSignInAsync(request.Email, request.Password, isPersistent: false, lockoutOnFailure: false);

        if (result.Succeeded)
        {
            Log.Information("LoginController: Пользователь {Email} авторизовался", request.Email);
            try
            {
                var token = jwtService.GenerateToken(request.Email);
                return Ok(new { Message = "Успешно вошли в систему.", AccessToken = token });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Ошибка генерации JWT токена для пользователя {Email}", request.Email);
                return StatusCode(500, new { ErrorMessage = "Произошла ошибка при генерации токена." });
            }
        }
        else
        {
            return Unauthorized(new { ErrorMessage = "LoginController: Неверные учетные данные." });
        }
    }

}