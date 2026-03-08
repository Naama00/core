using Microsoft.AspNetCore.Mvc;
using User.Models;
using MyApp.Services;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Security.Claims;

namespace MyApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly ILibraryBookService _libraryBookService;
        private readonly ICurrentUserService _currentUserService;

        public UsersController(IUserService userService, ILibraryBookService libraryBookService, ICurrentUserService currentUserService)
        {
            _userService = userService;
            _libraryBookService = libraryBookService;
            _currentUserService = currentUserService;
        }

        // 1. שליפת כל המשתמשים: GET /api/Users
        //    מנהלים מקבלים את כל הרשימה, משתמשים רגילים רואים רק את עצמם
        [HttpGet]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public ActionResult<List<User.Models.User>> GetAll()
        {
            try
            {
                if (_currentUserService.IsAdmin)
                {
                    var users = _userService.GetAll();
                    return Ok(users);
                }

                // לא מנהל: נחזיר רק את המשתמש הנוכחי
                var me = _currentUserService.User;
                if (me == null)
                {
                    return NotFound(new { message = "המשתמש לא נמצא" });
                }

                return Ok(new List<User.Models.User> { me });
            }
            catch (Exception ex)
            {
                // זה ידפיס לך בחלון ה-Output ב-Visual Studio את השגיאה האמיתית
                System.Diagnostics.Debug.WriteLine(ex.Message);

                return StatusCode(500, ex.Message);
            }
        }

        // 2. שליפת משתמש בודד לפי ID: GET /api/Users/{id}
        [HttpGet("{id}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public ActionResult<User.Models.User> GetById(int id)
        {
            var user = _userService.GetById(id);
            if (user == null)
            {
                return NotFound(new { message = $"משתמש עם מזהה {id} לא נמצא" });
            }
            return Ok(user);
        }

        // 3. הוספת משתמש חדש: POST /api/Users
        [HttpPost]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
        public ActionResult<User.Models.User> Create([FromBody] User.Models.User newUser)
        {
            if (newUser == null)
            {
                return BadRequest("נתוני משתמש לא תקינים");
            }

            _userService.Add(newUser);

            // מחזיר סטטוס 201 (Created) עם נתיב לשליפת האובייקט החדש
            return CreatedAtAction(nameof(GetById), new { id = newUser.Id }, newUser);
        }

        // 4. עדכון משתמש קיים: PUT /api/Users/{id}
        [HttpPut("{id}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public IActionResult Update(int id, [FromBody] User.Models.User updatedUser)
        {
            // 1. בדיקת קיום המשתמש
            var existingUser = _userService.GetById(id);
            if (existingUser == null)
            {
                return NotFound(new { message = $"משתמש עם מזהה {id} לא נמצא" });
            }

            // 2. בדיקת הרשאות (מי מנסה לעדכן?)
            if (_currentUserService.UserId != id && !_currentUserService.IsAdmin)
            {
                // אם המשתמש אינו מנהל ואינו המשתמש בעל ה-ID הזה
                return Forbid(); // 403 Forbidden
            }

            // 3. ביצוע העדכון
            _userService.Update(id, updatedUser);

            return NoContent(); // 204 No Content
        }

        // 5. מחיקת משתמש: DELETE /api/Users/{id}
        [HttpDelete("{id}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
        public IActionResult Delete(int id)
        {
            var user = _userService.GetById(id);
            if (user == null)
            {
                return NotFound();
            }
            
            // מחיקת כל הספרים של המשתמש (cascade delete)
            _libraryBookService.DeleteBooksByUserId(id);
            
            // מחיקת המשתמש עצמו
            _userService.Delete(id);
            return Ok(new { message = "המשתמש ניתן כל הספרים שלו נמחקו" });
        }


    }

}