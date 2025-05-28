// ConnectFour/Services/UserService.cs
using ConnectFour.Models;
using ConnectFour.Helpers; // Для PasswordHelper
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json; // Для работы с JSON

namespace ConnectFour.Services
{
    public class UserService
    {
        private readonly string _filePath = "users.json"; // Файл будет в папке с exe
        private List<User> _users;

        public UserService()
        {
            _users = LoadUsers();
        }

        private List<User> LoadUsers()
        {
            if (!File.Exists(_filePath))
            {
                return new List<User>();
            }
            try
            {
                string json = File.ReadAllText(_filePath);
                // Добавим проверку на пустой или некорректный JSON
                if (string.IsNullOrWhiteSpace(json)) return new List<User>();
                return JsonSerializer.Deserialize<List<User>>(json) ?? new List<User>();
            }
            catch
            {
                return new List<User>();
            }
        }
        public List<User> GetAllUsers()
        {
            // Возвращаем копию списка, чтобы внешний код не мог изменить оригинальный список _users
            // Или, если _users всегда актуален (перезагружается при необходимости), можно и так:
            return _users != null ? new List<User>(_users) : new List<User>();
            // Учитывая, что _users загружается в конструкторе, он не должен быть null.
            // return new List<User>(_users); // Можно и так, если _users всегда инициализирован
        }

        private void SaveUsers()
        {
            string json = JsonSerializer.Serialize(_users, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_filePath, json);
        }

        public bool RegisterUser(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
                return false;

            if (_users.Any(u => u.Username.Equals(username, System.StringComparison.OrdinalIgnoreCase)))
            {
                // Пользователь с таким именем уже существует
                return false;
            }

            var newUser = new User
            {
                Username = username,
                PasswordHash = PasswordHelper.HashPassword(password)
            };
            _users.Add(newUser);
            SaveUsers();
            return true;
        }

        public User AuthenticateUser(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
                return null;

            var user = _users.FirstOrDefault(u => u.Username.Equals(username, System.StringComparison.OrdinalIgnoreCase));
            if (user == null)
            {
                return null; // Пользователь не найден
            }

            string HashedPassword = PasswordHelper.HashPassword(password);
            if (user.PasswordHash == HashedPassword)
            {
                return user; // Аутентификация успешна
            }
            return null; // Пароль неверный
        }
    }
}