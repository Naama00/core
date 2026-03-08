using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using User.Models; // המודל שלך
using MyApp.Services; // ה-Namespace שבו נמצא השירות שלך
using System.Collections.Generic;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;

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
            if (loginModel == null || string.IsNullOrEmpty(loginModel.Email) || string.IsNullOrEmpty(loginModel.Password))
            {
                return BadRequest("אימייל וסיסמה נדרשים");
            }

            // 1. בדיקת משתמש מול ה-JSON
            var user = _userService.AuthenticateByEmail(loginModel.Email, loginModel.Password);

            if (user == null)
            {
                return Unauthorized("אימייל או סיסמה שגויים");
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

        [HttpGet("login-google")]
        public IActionResult LoginGoogle()
        {
            var properties = new AuthenticationProperties { RedirectUri = Url.Action("GoogleResponse") };
            return Challenge(properties, GoogleDefaults.AuthenticationScheme);
        }

        [HttpGet("google-response")]
        public async Task<IActionResult> GoogleResponse()
        {
            var result = await HttpContext.AuthenticateAsync(GoogleDefaults.AuthenticationScheme);
            
            if (!result.Succeeded)
                return Redirect("/login.html?error=authentication_failed");

            var claims = result.Principal?.Identities.FirstOrDefault()?.Claims;
            if (claims == null)
                return Redirect("/login.html?error=no_claims");

            // הדפסת כל ה-Claims לצפייה במה שמגיע מ-Google
            Console.WriteLine("=== Google Claims ===");
            foreach (var claim in claims)
            {
                Console.WriteLine($"Type: {claim.Type}, Value: {claim.Value}");
            }
            Console.WriteLine("===================");

            // חילוץ מידע מ-Google
            var email = claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
            var name = claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;
            var googleId = claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            
            // Extract givenname and surname from Google
            var givenName = claims.FirstOrDefault(c => c.Type == ClaimTypes.GivenName)?.Value;
            var surName = claims.FirstOrDefault(c => c.Type == ClaimTypes.Surname)?.Value;
            
            // Search for picture claim
            var profilePicture = claims.FirstOrDefault(c => c.Type == "picture")?.Value;

            Console.WriteLine("=== Google Response ===");
            Console.WriteLine($"Name: {name}");
            Console.WriteLine($"Given Name: {givenName}");
            Console.WriteLine($"Surname: {surName}");
            Console.WriteLine($"Email: {email}");
            Console.WriteLine($"Picture: {profilePicture}");
            Console.WriteLine("=======================");

            if (string.IsNullOrEmpty(name))
                return Redirect("/login.html?error=no_name");

            // בדיקה אם המשתמש כבר קיים במערכת (חיפוש לפי אימייל)
            var existingUser = _userService.GetAll()
                .FirstOrDefault(u => !string.IsNullOrEmpty(email) && u.Email == email);
            
            User.Models.User user;
            if (existingUser == null)
            {
                // יצירת משתמש חדש מ-Google
                user = new User.Models.User
                {
                    Name = name,
                    Email = email,
                    Password = googleId, // שמירת Google ID כ"סיסמה"
                    Role = "User",
                    ProfilePictureUrl = profilePicture
                };
                _userService.Add(user);
            }
            else
            {
                user = existingUser;
                // עדכן את האימייל אם הוא השתנה
                if (!string.IsNullOrEmpty(email) && user.Email != email)
                {
                    user.Email = email;
                }
                // עדכן את תמונת הפרופיל
                if (!string.IsNullOrEmpty(profilePicture))
                {
                    user.ProfilePictureUrl = profilePicture;
                }
                _userService.Update(user.Id, user);
            }

            // יצירת ה-Claims שלנו עבור JWT
            var jwtClaims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Name),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Role, user.Role)
            };
            
            // Add Google name parts if available
            if (!string.IsNullOrEmpty(givenName))
            {
                jwtClaims.Add(new Claim(ClaimTypes.GivenName, givenName));
                Console.WriteLine($"Added givenname claim to JWT: {givenName}");
            }
            if (!string.IsNullOrEmpty(surName))
            {
                jwtClaims.Add(new Claim(ClaimTypes.Surname, surName));
                Console.WriteLine($"Added surname claim to JWT: {surName}");
            }
            
            // Add picture URL as a claim if available
            if (!string.IsNullOrEmpty(user.ProfilePictureUrl))
            {
                jwtClaims.Add(new Claim("picture", user.ProfilePictureUrl));
                Console.WriteLine($"Added picture claim to JWT: {user.ProfilePictureUrl}");
            }

            // יצירת JWT טוקן
            var token = LibraryTokenService.GetToken(jwtClaims);
            var tokenString = LibraryTokenService.WriteToken(token);

            // הפניה חזרה לdaf login.html עם הטוקן ב-query string
            // The picture will now be in the JWT token itself 
            Console.WriteLine($"JWT token created with picture claim (if available)");
            return Redirect($"/login.html?token={tokenString}&role={user.Role}");
        }
    }

    // DTO לבקשת התחברות
    public class LoginRequest
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }
}