using System.Collections.Generic;
using User.Models;

namespace Library.Services
{
    public interface IUserService
    {
        User.Models.User? Authenticate(string name, string password);
        User.Models.User? AuthenticateByEmail(string email, string password);
        List<User.Models.User> GetAll();
        User.Models.User? GetById(int id);
        void Add(User.Models.User newUser);
        void Update(int id, User.Models.User updatedUser);
        void Delete(int id);
    }
}