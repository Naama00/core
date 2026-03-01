using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using User.Models; // המודל שלך
using MyApp.Services; // ה-Namespace שבו נמצא השירות שלך
using System.Collections.Generic;

namespace Library.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IUserService _userService;

        public AuthController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpPost("login")]
        public ActionResult<object> Login([FromBody] LoginRequest loginModel)
        {
            if (loginModel == null || string.IsNullOrEmpty(loginModel.Name) || string.IsNullOrEmpty(loginModel.Password))
            {
                return BadRequest("שם משתמש וסיסמה נדרשים");
            }

            // 1. בדיקת משתמש מול ה-JSON
            var user = _userService.Authenticate(loginModel.Name, loginModel.Password);

            if (user == null)
            {
                return Unauthorized("שם משתמש או סיסמה שגויים");
            }

            // 2. יצירת ה-Claims (מידע על המשתמש)
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Name),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                // הוספת התפקיד (Admin/User) לטוקן
                new Claim(ClaimTypes.Role, user.Role) 
            };

            // 3. יצירת הטוקן באמצעות השירות של הספריה
            var token = LibraryTokenService.GetToken(claims);
            var tokenString = LibraryTokenService.WriteToken(token);

            // מחזירים את הטוקן למשתמש
            return Ok(new { Token = tokenString, Username = user.Name, Role = user.Role });
        }
    }

    // DTO לבקשת התחברות
    public class LoginRequest
    {
        public string Name { get; set; }
        public string Password { get; set; }
    }
}