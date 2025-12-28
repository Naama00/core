using System.Collections.Generic;
using User.Models;

namespace MyApp.Services
{
    public interface IUserService
    {
        List<User.Models.User> GetAll();
        User.Models.User? GetById(int id);
        void Add(User.Models.User newUser);
        void Update(int id, User.Models.User updatedUser); // הוספה
        void Delete(int id); // הוספה
    }
}