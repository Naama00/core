using Microsoft.AspNetCore.Mvc;
using User.Models;
using MyApp.Services;
using System.Collections.Generic;

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
        [HttpGet]
        public ActionResult<List<User.Models.User>> GetAll()
        {
           try 
    {
        var users = _userService.GetAll();
        return Ok(users);
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
            public IActionResult Update(int id, [FromBody] User.Models.User updatedUser)
            {
                var user = _userService.GetById(id);
                if (user == null) return NotFound();

                _userService.Update(id, updatedUser);
                return NoContent(); // מחזיר 204 בהצלחה
            }

            // 5. מחיקת משתמש: DELETE /api/Users/{id}
            [HttpDelete("{id}")]
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