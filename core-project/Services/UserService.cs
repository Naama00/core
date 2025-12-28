using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using User.Models;

namespace MyApp.Services
{
    public class UserService : IUserService
    {
        private readonly string _filePath;

        // הזרקת IWebHostEnvironment מאפשרת לנו למצוא את הנתיב האמיתי של התיקייה
        public UserService(IWebHostEnvironment env)
        {
            // יצירת נתיב מוחלט לתיקיית Data בשורש הפרויקט
            _filePath = Path.Combine(env.ContentRootPath, "Data", "users.json");

            // וודוא שהתיקייה קיימת כדי למנוע קריסה בשמירה הראשונה
            var directory = Path.GetDirectoryName(_filePath);
            if (directory != null && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
        }

        private List<User.Models.User> LoadData()
        {
            try
            {
                if (!File.Exists(_filePath))
                {
                    return new List<User.Models.User>();
                }

                string json = File.ReadAllText(_filePath);
                if (string.IsNullOrWhiteSpace(json)) return new List<User.Models.User>();

                // הגדרת PropertyNameCaseInsensitive פותרת בעיות של תאימות בין JS ל-C#
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                return JsonSerializer.Deserialize<List<User.Models.User>>(json, options) ?? new List<User.Models.User>();
            }
            catch (Exception ex)
            {
                // במקרה של שגיאה בקריאת הקובץ, נחזיר רשימה ריקה ולא נגרום לכל השרת לקרוס
                Console.WriteLine($"Error reading JSON: {ex.Message}");
                return new List<User.Models.User>();
            }
        }

        private void SaveData(List<User.Models.User> users)
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            string json = JsonSerializer.Serialize(users, options);
            File.WriteAllText(_filePath, json);
        }

        public List<User.Models.User> GetAll() => LoadData();

        public User.Models.User? GetById(int id) => LoadData().FirstOrDefault(u => u.Id == id);

        public void Add(User.Models.User newUser)
        {
            var users = LoadData();
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
            users.RemoveAll(u => u.Id == id);
            SaveData(users);
        }
    }
}