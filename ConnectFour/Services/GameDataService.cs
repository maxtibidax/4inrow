// ConnectFour/Services/GameDataService.cs
using ConnectFour.Logic.Models;
using System.IO;
using System.Text.Json;

namespace ConnectFour.Services
{
    public class GameDataService
    {
        private string GetFilePath(string username)
        {
            // Файл будет в папке с exe, например "username_gamedata.json"
            return $"{username}_gamedata.json";
        }

        public void SaveGameData(string username, UserGameData data)
        {
            if (string.IsNullOrWhiteSpace(username) || data == null)
                return;

            try
            {
                string filePath = GetFilePath(username);
                string json = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(filePath, json);
            }
            catch (System.Exception ex)
            {
                // Здесь можно добавить логирование ошибки, если нужно
                System.Diagnostics.Debug.WriteLine($"Error saving game data for {username}: {ex.Message}");
            }
        }

        public UserGameData LoadGameData(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
                return null;

            try
            {
                string filePath = GetFilePath(username);
                if (!File.Exists(filePath))
                {
                    return null; // Нет сохраненных данных для этого пользователя
                }

                string json = File.ReadAllText(filePath);
                return JsonSerializer.Deserialize<UserGameData>(json);
            }
            catch (System.Exception ex)
            {
                // Логирование ошибки
                System.Diagnostics.Debug.WriteLine($"Error loading game data for {username}: {ex.Message}");
                return null; // В случае ошибки возвращаем null
            }
        }
    }
}