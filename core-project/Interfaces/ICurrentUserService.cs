using User.Models;

namespace MyApp.Services
{
    public interface ICurrentUserService
    {
        int UserId { get; }
        string UserName { get; }
        string UserRole { get; }
        User.Models.User? User { get; }
        bool IsAuthenticated { get; }
        bool IsAdmin { get; }
    }
}
