// ConnectFour/App.xaml.cs
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using ConnectFour.View;
using ConnectFour.ViewModel; // Для AuthViewModel и MainWindowViewModel

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

            // Убедись, что StartupUri не задан в App.xaml

            AuthWindow authWindow = new AuthWindow();

            // Получаем или создаем AuthViewModel
            AuthViewModel authViewModel = authWindow.DataContext as AuthViewModel;
            if (authViewModel == null)
            {
                authViewModel = new AuthViewModel();
                authWindow.DataContext = authViewModel;
            }

            bool loginSuccessful = false;
            string loggedInUsername = null; // Для хранения имени пользователя
            MainWindow mainWindow = null;

            // Подписываемся на событие успешного входа
            // args в лямбде теперь будет типа LoginSuccessEventArgs
            authViewModel.LoginSuccess += (sender, args) =>
            {
                loginSuccessful = true;
                loggedInUsername = args.Username; // Получаем имя пользователя из аргументов события

                // Создаем главное окно и его ViewModel в UI потоке
                Application.Current.Dispatcher.Invoke(() =>
                {
                    // Сначала создаем ViewModel, передавая ему имя пользователя
                    var mainVM = new MainWindowViewModel(loggedInUsername);

                    // Затем создаем MainWindow и устанавливаем его DataContext
                    mainWindow = new MainWindow
                    {
                        DataContext = mainVM
                    };

                    // Закрываем окно аутентификации с положительным результатом
                    authWindow.DialogResult = true;
                });
            };

            // Показываем окно аутентификации как модальное
            bool? dialogResult = authWindow.ShowDialog();

            // Проверяем результат аутентификации
            if (dialogResult == true && loginSuccessful && mainWindow != null && !string.IsNullOrEmpty(loggedInUsername))
            {
                // Если вход успешен и главное окно создано, показываем его
                mainWindow.Show();
            }
            else
            {
                // Если аутентификация не удалась или пользователь закрыл окно,
                // завершаем приложение
                Current.Shutdown();
            }
        }
    }
}