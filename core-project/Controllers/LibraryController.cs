using Microsoft.AspNetCore.Mvc;
using System.IO;
using Library.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Security.Claims;
using MyApp.Services;
using Microsoft.AspNetCore.SignalR;
using Library.Hubs;

namespace Library.Controllers
{
    [ApiController]
    [Route("LibraryBook")]
    public class LibraryBookController : ControllerBase
    {
        private readonly ILibraryBookService _libraryBookService;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger<LibraryBookController> _logger;
        private readonly IHubContext<LibraryHub> _hubContext;

        public LibraryBookController(ILibraryBookService libraryBookService, ICurrentUserService currentUserService, ILogger<LibraryBookController> logger, IHubContext<LibraryHub> hubContext)
        {
            _libraryBookService = libraryBookService;
            _currentUserService = currentUserService;
            _logger = logger;
            _hubContext = hubContext;
        }

        [HttpGet]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public ActionResult<List<LibraryBook>> GetAll()
        {
            try
            {
                var allBooks = _libraryBookService.GetAllBooks();
                
                if (_currentUserService.IsAdmin)
                {
                    return Ok(allBooks);
                }
                
                var userBooks = allBooks
                    .Where(b => b.UserId == _currentUserService.UserId)
                    .ToList();
                
                return Ok(userBooks);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while getting all books.");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("{id}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public ActionResult<LibraryBook> Get(int id)
        {
            try
            {
                var book = _libraryBookService.GetBookById(id);
                
                if (book == null)
                {
                    return NotFound("Book not found");
                }

                if (!_currentUserService.IsAdmin && book.UserId != _currentUserService.UserId)
                {
                    return Forbid("You don't have access to this book");
                }

                return Ok(book);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while getting the book.");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")] 
        public async Task<ActionResult> Create([FromBody] LibraryBook newBook)
        {
            if (newBook == null || string.IsNullOrEmpty(newBook.Name))
            {
                return BadRequest("השרת לא הצליח לקרוא את נתוני הספר - ודאי שמות שדות תואמים");
            }

            _libraryBookService.AddBook(newBook);
            
            if (newBook.UserId > 0)
            {
                await _hubContext.Clients.All.SendAsync("BookAdded", newBook);
            }
            
            return Ok(newBook);
        }

        [HttpPut("{id}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")] 
        public async Task<ActionResult> Update(int id, [FromBody] LibraryBook newBook)
        {
            if (newBook == null || id != newBook.Id)
            {
                return BadRequest("Invalid book data or ID mismatch");
            }

            var existingBook = _libraryBookService.GetBookById(id);
            if (existingBook == null)
            {
                return NotFound();
            }

            _libraryBookService.UpdateBook(id, newBook);
            
            if (newBook.UserId > 0)
            {
                await _hubContext.Clients.All.SendAsync("BookUpdated", newBook);
            }
            
            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")] 
        public async Task<ActionResult> Delete(int id)
        {
            var book = _libraryBookService.GetBookById(id);
            if (book == null)
            {
                return NotFound();
            }

            _libraryBookService.DeleteBook(id);
            
            if (book.UserId > 0)
            {
                await _hubContext.Clients.All.SendAsync("BookDeleted", id);
            }
            
            return NoContent();
        }

        [HttpGet("view")]
        public IActionResult GetLibraryView()
        {
            string filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "index.html");
            if (!System.IO.File.Exists(filePath))
            {
                return NotFound("index.html not found in wwwroot");
            }
            return PhysicalFile(filePath, "text/html");
        }
    }
}