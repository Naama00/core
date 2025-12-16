
using Library.Models;
public interface ILibraryBookService

{

IEnumerable<LibraryBook> GetAllBooks();

LibraryBook GetBookById(int id);

void AddBook(LibraryBook book);

void UpdateBook(int id, LibraryBook book);

void DeleteBook(int id);

}