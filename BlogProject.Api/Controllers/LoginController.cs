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
public class LoginController(SignInManager<User> signInManager) : ControllerBase
{
    [HttpPost("login")]
    public async Task<ActionResult> Login(LoginRequest request)
    {

        var result = await signInManager.PasswordSignInAsync(request.Email, request.Password, isPersistent: false, lockoutOnFailure: false);

        if (result.Succeeded)
        {
            Log.Information("LoginController: Пользователь {Email} авторизовался", request.Email);
            return Ok(new { Message = "Успешно вошли в систему." });
        }
        else
        {
            return Unauthorized(new { ErrorMessage = "LoginController: Неверные учетные данные." });
        }
    }

    [HttpPost("logout")]
    public async Task<ActionResult> Logout()
    {
        var email = User.Identity?.Name;

        if (string.IsNullOrEmpty(email))
        {
            return BadRequest(new { ErrorMessage = "Нет авторизованных пользователей" });
        }

        await signInManager.SignOutAsync();
        Log.Information("LogoutController: Пользователь {Email} вышел из системы", email);
        return Ok(new { Message = "Logged out successfully." });
    }

}