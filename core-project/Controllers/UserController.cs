using Microsoft.AspNetCore.Mvc;
using User.Models;
using MyApp.Services;
using Library.Services;
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

                var me = _currentUserService.User;
                if (me == null)
                {
                    return NotFound(new { message = "המשתמש לא נמצא" });
                }

                return Ok(new List<User.Models.User> { me });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
                return StatusCode(500, ex.Message);
            }
        }

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

        [HttpPost]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
        public ActionResult<User.Models.User> Create([FromBody] User.Models.User newUser)
        {
            if (newUser == null)
            {
                return BadRequest("נתוני משתמש לא תקינים");
            }

            _userService.Add(newUser);

            return CreatedAtAction(nameof(GetById), new { id = newUser.Id }, newUser);
        }

        [HttpPut("{id}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public IActionResult Update(int id, [FromBody] User.Models.User updatedUser)
        {
            var existingUser = _userService.GetById(id);
            if (existingUser == null)
            {
                return NotFound(new { message = $"משתמש עם מזהה {id} לא נמצא" });
            }

            if (_currentUserService.UserId != id && !_currentUserService.IsAdmin)
            {
                return Forbid(); 
            }

            _userService.Update(id, updatedUser);

            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
        public IActionResult Delete(int id)
        {
            var user = _userService.GetById(id);
            if (user == null)
            {
                return NotFound();
            }
            
            _libraryBookService.DeleteBooksByUserId(id);
            
            _userService.Delete(id);
            return Ok(new { message = "המשתמש וכל הספרים שלו נמחקו" });
        }


    }

}