using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using User.Models;
using MyApp.Services;
using Library.Services;
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

            var user = _userService.AuthenticateByEmail(loginModel.Email, loginModel.Password);

            if (user == null)
            {
                return Unauthorized("אימייל או סיסמה שגויים");
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Name),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Role, user.Role) 
            };

            var token = LibraryTokenService.GetToken(claims);
            var tokenString = LibraryTokenService.WriteToken(token);

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

            Console.WriteLine("=== Google Claims ===");
            foreach (var claim in claims)
            {
                Console.WriteLine($"Type: {claim.Type}, Value: {claim.Value}");
            }
            Console.WriteLine("===================");

            var email = claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
            var name = claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;
            var googleId = claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            var givenName = claims.FirstOrDefault(c => c.Type == ClaimTypes.GivenName)?.Value;
            var surName = claims.FirstOrDefault(c => c.Type == ClaimTypes.Surname)?.Value;
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

            var existingUser = _userService.GetAll()
                .FirstOrDefault(u => !string.IsNullOrEmpty(email) && u.Email == email);
            
            User.Models.User user;
            if (existingUser == null)
            {
                user = new User.Models.User
                {
                    Name = name,
                    Email = email,
                    Password = googleId, 
                    Role = "User",
                    ProfilePictureUrl = profilePicture
                };
                _userService.Add(user);
            }
            else
            {
                user = existingUser;
                if (!string.IsNullOrEmpty(email) && user.Email != email)
                {
                    user.Email = email;
                }
                if (!string.IsNullOrEmpty(profilePicture))
                {
                    user.ProfilePictureUrl = profilePicture;
                }
                _userService.Update(user.Id, user);
            }

            var jwtClaims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Name),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Role, user.Role)
            };
            
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
            
            if (!string.IsNullOrEmpty(user.ProfilePictureUrl))
            {
                jwtClaims.Add(new Claim("picture", user.ProfilePictureUrl));
                Console.WriteLine($"Added picture claim to JWT: {user.ProfilePictureUrl}");
            }

            var token = LibraryTokenService.GetToken(jwtClaims);
            var tokenString = LibraryTokenService.WriteToken(token);

            Console.WriteLine($"JWT token created with picture claim (if available)");
            return Redirect($"/login.html?token={tokenString}&role={user.Role}");
        }
    }

    public class LoginRequest
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }
}