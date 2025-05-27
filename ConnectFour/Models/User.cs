// ConnectFour/Models/User.cs
namespace ConnectFour.Models
{
    public class User
    {
        public string Username { get; set; }
        public string PasswordHash { get; set; } // Будем хранить хеш пароля, а не сам пароль
    }
}