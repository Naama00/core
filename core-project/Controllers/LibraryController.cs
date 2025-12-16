using Microsoft.AspNetCore.Mvc;
using System.IO;
using Microsoft.AspNetCore.Mvc;
using Library.Models;

namespace Library.Controllers
{
    [ApiController]
    [Route("[controller]")]
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
            return book == null ? NotFound() : book;
        }

        [HttpPost]
        public ActionResult Create(LibraryBook newBook)
        {
            _libraryBookService.AddBook(newBook);
            return CreatedAtAction(nameof(Create), new { id = newBook.Id });
        }

        [HttpPut("{id}")]
        public ActionResult Update(int id, LibraryBook newBook)
        {
            var book = _libraryBookService.GetBookById(id);
            if (book == null)
                return NotFound();
            if (book.Id != newBook.Id)
                return BadRequest();

            _libraryBookService.UpdateBook(id, newBook);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            var book = _libraryBookService.GetBookById(id);
            if (book == null)
                return NotFound();

            _libraryBookService.DeleteBook(id);
            return NoContent();
        }

        [HttpGet("view")]
        public IActionResult GetLibraryView()
        {
            return PhysicalFile(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "index.html"), "text/html");
        }
    }
    
}