using BlogProject.Data.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace BlogProject.Api.Controllers;

[AllowAnonymous]
[Route("api/[controller]")]
[ApiController]
public class LoginController(SignInManager<User> signInManager) : ControllerBase
{
    [HttpPost("login")]
    public async Task<ActionResult> Login([FromBody] LoginRequest request)
    {

        var result = await signInManager.PasswordSignInAsync(request.Email, request.Password, isPersistent: false, lockoutOnFailure: false);

        if (result.Succeeded)
        {
            Log.Information("LoginController: Пользователь {Email} авторизовался", request.Email);
            return Ok(new { Message = "Logged in successfully." });
        }
        else
        {
            return Unauthorized(new { ErrorMessage = "LoginController: Invalid credentials." });
        }
    }

    [HttpPost("logout")]
    public async Task<ActionResult> Logout()
    {
        var email = User.Identity?.Name;
        await signInManager.SignOutAsync();
        Log.Information("Пользователь {Email} вышел из системы", email);
        return Ok(new { Message = "Logged out successfully." });
    }

}