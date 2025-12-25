using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Library.Models;

public class LibraryBookService : ILibraryBookService
{
    private readonly string _filePath = "Data/books.json";

    // פונקציית עזר לטעינת נתונים
    private List<LibraryBook> LoadData()
    {
        if (!File.Exists(_filePath))
        {
            // אם הקובץ לא קיים, ניצור אותו עם נתוני ברירת המחדל שלך
            var defaultBooks = new List<LibraryBook>
            {
                new LibraryBook{Id=1, Name="Assasin", WriterName="Sappir", IsBorrowed=true, IsForAdults=true},
                new LibraryBook{Id=2, Name="Pdahel", WriterName="Keinan", IsBorrowed=false, IsForAdults=false},
                new LibraryBook{Id=3, Name="Duplicates", WriterName="Sappir", IsBorrowed=true, IsForAdults=true}    
            };
            SaveData(defaultBooks);
            return defaultBooks;
        }

        string json = File.ReadAllText(_filePath);
        if (string.IsNullOrWhiteSpace(json)) return new List<LibraryBook>();

        return JsonSerializer.Deserialize<List<LibraryBook>>(json) ?? new List<LibraryBook>();
    }

    // פונקציית עזר לשמירת נתונים
    private void SaveData(List<LibraryBook> books)
    {
        var options = new JsonSerializerOptions { WriteIndented = true };
        string json = JsonSerializer.Serialize(books, options);
        File.WriteAllText(_filePath, json);
    }

    public IEnumerable<LibraryBook> GetAllBooks() => LoadData();

    public LibraryBook GetBookById(int id) => LoadData().FirstOrDefault(b => b.Id == id);

    public void AddBook(LibraryBook book)
    {
        var booksList = LoadData();
        book.Id = booksList.Any() ? booksList.Max(b => b.Id) + 1 : 1;
        booksList.Add(book);
        SaveData(booksList);
    }

    public void UpdateBook(int id, LibraryBook book)
    {
        var booksList = LoadData();
        var index = booksList.FindIndex(b => b.Id == id);
        if (index != -1)
        {
            book.Id = id; // מוודאים שה-ID נשאר זהה
            booksList[index] = book;
            SaveData(booksList);
        }
    }

    public void DeleteBook(int id)
    {
        var booksList = LoadData();
        var book = booksList.FirstOrDefault(b => b.Id == id);
        if (book != null)
        {
            booksList.Remove(book);
            SaveData(booksList);
        }
    }
}