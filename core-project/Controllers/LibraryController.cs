using Microsoft.AspNetCore.Mvc;
using System.IO;
using Library.Models;


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
        public ActionResult<LibraryBook> Get(int id)
        {
            var book = _libraryBookService.GetBookById(id);
            return book == null ? NotFound() : Ok(book);
        }

        [HttpPost]
        public ActionResult Create([FromBody] LibraryBook newBook)
        {
            // אם ה-JS שלח אותיות קטנות והשרת לא הוגדר נכון, newBook יהיה null או עם שדות ריקים
            if (newBook == null || string.IsNullOrEmpty(newBook.Name))
            {
                return BadRequest("השרת לא הצליח לקרוא את נתוני הספר - ודאי שמות שדות תואמים");
            }

            _libraryBookService.AddBook(newBook);
            return Ok(newBook);
        }

        [HttpPut("{id}")]
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
            // ודאי שהקובץ index.html באמת נמצא בתוך תיקיית wwwroot
            string filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "index.html");
            if (!System.IO.File.Exists(filePath))
            {
                return NotFound("index.html not found in wwwroot");
            }
            return PhysicalFile(filePath, "text/html");
        }
    }
}