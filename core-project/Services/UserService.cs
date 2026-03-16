using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using User.Models;

namespace Library.Services
{
    public class UserService : IUserService
    {
        private readonly string _filePath;

        public UserService(IWebHostEnvironment env)
        {
            _filePath = Path.Combine(env.ContentRootPath, "Data", "users.json");

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

                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                return JsonSerializer.Deserialize<List<User.Models.User>>(json, options) ?? new List<User.Models.User>();
            }
            catch (Exception ex)
            {
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

        public User.Models.User? Authenticate(string name, string password)
        {
            var users = LoadData();
            return users.FirstOrDefault(u => u.Name == name && u.Password == password);
        }

        public User.Models.User? AuthenticateByEmail(string email, string password)
        {
            var users = LoadData();
            return users.FirstOrDefault(u => !string.IsNullOrEmpty(u.Email) && u.Email == email && u.Password == password);
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
                if (!string.IsNullOrEmpty(updatedUser.Email))
                {
                    existingUser.Email = updatedUser.Email;
                }
                if (updatedUser.ProfilePictureUrl != null)
                {
                    existingUser.ProfilePictureUrl = updatedUser.ProfilePictureUrl;
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