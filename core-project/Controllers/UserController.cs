using Microsoft.AspNetCore.Mvc;
using User.Models;
using MyApp.Services;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace MyApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        // 1. שליפת כל המשתמשים: GET /api/Users
        //    מנהלים מקבלים את כל הרשימה, משתמשים רגילים רואים רק את עצמם
        [HttpGet]
        [Authorize]
        public ActionResult<List<User.Models.User>> GetAll()
        {
            try
            {
                bool isAdmin = User.IsInRole("Admin");

                if (isAdmin)
                {
                    var users = _userService.GetAll();
                    return Ok(users);
                }

                // לא מנהל: נחזיר רק את המשתמש הנוכחי
                // רישום Claims כדי לאבחן בעיות
                var claimsList = User.Claims.Select(c => $"{c.Type}={c.Value}");
                System.Diagnostics.Debug.WriteLine("Current user claims: " + string.Join(", ", claimsList));

                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                                   ?? User.FindFirst("sub")?.Value; // jwt mapping

                if (!int.TryParse(userIdClaim, out int currentUserId))
                {
                    // אם אין ID תקין נחשב כלא מחובר
                    return Unauthorized();
                }

                var me = _userService.GetById(currentUserId);
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
        [Authorize]
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
        [Authorize(Roles = "Admin")]
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
        [Authorize]
        public IActionResult Update(int id, [FromBody] User.Models.User updatedUser)
        {
            // 1. בדיקת קיום המשתמש
            var existingUser = _userService.GetById(id);
            if (existingUser == null)
            {
                return NotFound(new { message = $"משתמש עם מזהה {id} לא נמצא" });
            }

            // 2. בדיקת הרשאות (מי מנסה לעדכן?)
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            bool isAdmin = User.IsInRole("Admin");

            // המרה בטוחה של ה-Claim ל-int
            if (!int.TryParse(userIdClaim, out int currentUserId) || (currentUserId != id && !isAdmin))
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
        [Authorize(Roles = "Admin")]
        public IActionResult Delete(int id)
        {
            var user = _userService.GetById(id);
            if (user == null)
            {
                return NotFound();
            }
            _userService.Delete(id); // עכשיו זה יעבוד!
            return Ok(new { message = "המשתמש נמחק" });
        }


    }

}