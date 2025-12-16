using Library.Models;
public class LibraryBookService : ILibraryBookService

{

    private readonly List<LibraryBook> booksList;


    public LibraryBookService()

    {

        booksList = new List<LibraryBook>

{

new LibraryBook{Id=1, Name="Assasin", WriterName="Sappir", IsBorrowed=true},

new LibraryBook{Id=2, Name="Pdahel", WriterName="Keinan", IsBorrowed=false},

new LibraryBook{Id=3, Name="Duplicates", WriterName="Sappir", IsBorrowed=true}

};

    }


    public IEnumerable<LibraryBook> GetAllBooks() => booksList;


    public LibraryBook GetBookById(int id) => booksList.FirstOrDefault(b => b.Id == id);


    public void AddBook(LibraryBook book)

    {

        book.Id = booksList.Max(b => b.Id) + 1;

        booksList.Add(book);

    }


    public void UpdateBook(int id, LibraryBook book)

    {

        var existingBook = GetBookById(id);

        if (existingBook != null)

        {

            booksList[booksList.IndexOf(existingBook)] = book;

        }

    }


    public void DeleteBook(int id)

    {

        var book = GetBookById(id);

        if (book != null)

        {

            booksList.Remove(book);

        }

    }

}