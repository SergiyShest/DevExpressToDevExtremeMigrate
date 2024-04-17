using GeneratedUI.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Security.Claims;


namespace UI.Areas.Account.Controllers
{
    //[ApiController]
    [Area("Account")]
    public class LoginController : Controller
    {
      [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string username, string password)
        {
            // Здесь должна быть проверка username и password
            if (!string.IsNullOrEmpty(username) ) // Тестовые данные
            {
                var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, username)
            };
                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, claimsPrincipal);

              return  RedirectToAction("Index", "Home", new { area = "" });
        
            }

            return Unauthorized("Неправильный логин или пароль");
        }
    }
}