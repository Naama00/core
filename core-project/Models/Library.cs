namespace Library.Models
{
    public class LibraryBook
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string WriterName { get; set; }
        public bool IsBorrowed { get; set; }
        public bool IsForAdults { get; set; }
    }
}