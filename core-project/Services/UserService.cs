using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using User.Models;

namespace MyApp.Services
{
    public class UserService : IUserService
    {
        // נתיב לקובץ ה-JSON - יישמר בתיקיית הפרויקט
        private readonly string _filePath = "users.json";

        // פונקציית עזר פרטית לקריאת הרשימה מהקובץ
        private List<User.Models.User> LoadData()
        {
            if (!File.Exists(_filePath))
            {
                return new List<User.Models.User>();
            }

            string json = File.ReadAllText(_filePath);
            
            // אם הקובץ ריק, נחזיר רשימה חדשה
            if (string.IsNullOrWhiteSpace(json)) return new List<User.Models.User>();

            return JsonSerializer.Deserialize<List<User.Models.User>>(json) ?? new List<User.Models.User>();
        }

        // פונקציית עזר פרטית לשמירת הרשימה לקובץ
        private void SaveData(List<User.Models.User> users)
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            string json = JsonSerializer.Serialize(users, options);
            File.WriteAllText(_filePath, json);
        }

        public List<User.Models.User> GetAll()
        {
            return LoadData();
        }

        public User.Models.User GetById(int id)
        {
            return LoadData().FirstOrDefault(u => u.Id == id);
        }

        public void Add(User.Models.User newUser)
        {
            var users = LoadData();
            // יצירת מזהה אוטומטי
            newUser.Id = users.Any() ? users.Max(u => u.Id) + 1 : 1;
            users.Add(newUser);
            SaveData(users);
        }

        public void Update(int id, User.Models.User updatedUser)
        {
            var users = LoadData();
            var existingUser = users.FirstOrDefault(u => u.Id == id);
            
            if (existingUser != null)
            {
                existingUser.Name = updatedUser.Name;
                if (!string.IsNullOrEmpty(updatedUser.Password))
                {
                    existingUser.Password = updatedUser.Password;
                }
                SaveData(users);
            }
        }

        public void Delete(int id)
        {
            var users = LoadData();
            var user = users.FirstOrDefault(u => u.Id == id);
            
            if (user != null)
            {
                users.RemoveAll(u => u.Id == id);
                SaveData(users);
            }
        }
    }
}