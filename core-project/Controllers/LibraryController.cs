using Microsoft.AspNetCore.Mvc;
using System.IO;
using Library.Models;
// --- הוספת ה-Namespace הנדרש להרשאות ---
using Microsoft.AspNetCore.Authorization;
// ----------------------------------------

namespace Library.Controllers
{
    [ApiController]
    [Route("LibraryBook")]
    public class LibraryBookController : ControllerBase
    {
        private readonly ILibraryBookService _libraryBookService;
        private readonly ILogger<LibraryBookController> _logger;

        public LibraryBookController(ILibraryBookService libraryBookService, ILogger<LibraryBookController> logger)
        {
            _libraryBookService = libraryBookService;
            _logger = logger;
        }

        [HttpGet]
        [Authorize]
        public ActionResult<List<LibraryBook>> GetAll()
        {
            try
            {
                var books = _libraryBookService.GetAllBooks();
                return Ok(books);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while getting all books.");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("{id}")]
        [Authorize]
        public ActionResult<LibraryBook> Get(int id)
        {
            var book = _libraryBookService.GetBookById(id);
            return book == null ? NotFound() : Ok(book);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")] // <--- רק מנהל יכול להוסיף ספרים!
        public ActionResult Create([FromBody] LibraryBook newBook)
        {
            if (newBook == null || string.IsNullOrEmpty(newBook.Name))
            {
                return BadRequest("השרת לא הצליח לקרוא את נתוני הספר - ודאי שמות שדות תואמים");
            }

            _libraryBookService.AddBook(newBook);
            return Ok(newBook);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")] // <--- רק מנהל יכול לעדכן ספרים!
        public ActionResult Update(int id, [FromBody] LibraryBook newBook)
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
            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")] // <--- רק מנהל יכול למחוק ספרים!
        public ActionResult Delete(int id)
        {
            var book = _libraryBookService.GetBookById(id);
            if (book == null)
            {
                return NotFound();
            }

            _libraryBookService.DeleteBook(id);
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