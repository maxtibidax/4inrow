using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using ConnectFour.View;
using ConnectFour.ViewModel;

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

            // Получаем ViewModel
            AuthViewModel authViewModel = authWindow.DataContext as AuthViewModel;
            if (authViewModel == null)
            {
                authViewModel = new AuthViewModel();
                authWindow.DataContext = authViewModel;
            }

            // Флаг для отслеживания успешного входа
            bool loginSuccessful = false;
            MainWindow mainWindow = null;

            // Подписываемся на событие успешного входа
            authViewModel.LoginSuccess += (sender, args) =>
            {
                loginSuccessful = true;

                // Создаем главное окно в том же потоке
                Application.Current.Dispatcher.Invoke(() =>
                {
                    mainWindow = new MainWindow();
                    // Закрываем окно аутентификации
                    authWindow.DialogResult = true;
                });
            };

            // Показываем окно аутентификации как модальное
            bool? dialogResult = authWindow.ShowDialog();

            // Проверяем результат
            if (dialogResult == true && loginSuccessful && mainWindow != null)
            {
                // Показываем главное окно
                mainWindow.Show();
            }
            else
            {
                // Если аутентификация не удалась, завершаем приложение
                Current.Shutdown();
            }
        }
    }
}