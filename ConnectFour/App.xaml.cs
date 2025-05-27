using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using ConnectFour.View;      // <--- ДОБАВЬ ЭТО (для AuthWindow и MainWindow)
using ConnectFour.ViewModel; // <--- ДОБАВЬ ЭТО (для AuthViewModel)

namespace ConnectFour
{
    /// <summary>
    /// Logika interakcji dla klasy App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Создаем окно аутентификации
            AuthWindow authWindow = new AuthWindow();

            // Получаем или создаем ViewModel для окна аутентификации
            // Если ты устанавливаешь DataContext в XAML AuthWindow, эта проверка может быть излишней,
            // но это хороший способ убедиться, что ViewModel существует.
            AuthViewModel authViewModel = authWindow.DataContext as AuthViewModel;
            if (authViewModel == null)
            {
                authViewModel = new AuthViewModel();
                authWindow.DataContext = authViewModel;
            }

            bool loginSuccessful = false;

            // Подписываемся на событие успешного входа
            authViewModel.LoginSuccess += (sender, args) =>
            {
                loginSuccessful = true;
                authWindow.Close(); // Закрываем окно аутентификации
            };

            // Показываем окно аутентификации как модальное (блокирует остальную часть приложения)
            // Метод ShowDialog() вернет управление только после закрытия окна.
            // Можно также проверить authWindow.ShowDialog() == true, если AuthWindow.DialogResult устанавливается.
            // Но для простоты используем флаг loginSuccessful.
            authWindow.ShowDialog();

            if (loginSuccessful)
            {
                // Если вход успешен, показываем главное окно
                MainWindow mainWindow = new MainWindow();
                // Здесь ты можешь также настроить ViewModel для MainWindow, если он у тебя есть
                // Например:
                // var mainViewModel = new MainViewModel(); // или как он у тебя называется
                // mainWindow.DataContext = mainViewModel;
                mainWindow.Show();
            }
            else
            {
                // Если пользователь закрыл окно аутентификации без успешного входа,
                // или аутентификация не удалась, завершаем приложение.
                Current.Shutdown();
            }
        }
    }
}