// ConnectFour/App.xaml.cs
using ConnectFour.View;
using ConnectFour.ViewModel;
using System.Windows;

namespace ConnectFour
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            ShutdownMode = ShutdownMode.OnExplicitShutdown; // Важно, чтобы приложение не закрывалось при закрытии последнего окна, если мы управляем этим циклом

            bool keepRunning = true;
            while (keepRunning)
            {
                keepRunning = false; // По умолчанию, если не будет успешного логина или выхода для повтора

                AuthWindow authWindow = new AuthWindow();
                AuthViewModel authViewModel = authWindow.DataContext as AuthViewModel;
                if (authViewModel == null)
                {
                    authViewModel = new AuthViewModel();
                    authWindow.DataContext = authViewModel;
                }

                bool loginSuccessful = false;
                string loggedInUsername = null;
                MainWindow mainWindow = null;

                authViewModel.LoginSuccess += (sender, args) =>
                {
                    loginSuccessful = true;
                    loggedInUsername = args.Username;
                    authWindow.DialogResult = true; // Закрываем AuthWindow с успехом
                };

                bool? authDialogResult = authWindow.ShowDialog();

                if (authDialogResult == true && loginSuccessful && !string.IsNullOrEmpty(loggedInUsername))
                {
                    // Успешный вход
                    var mainVM = new MainWindowViewModel(loggedInUsername);
                    mainWindow = new MainWindow
                    {
                        DataContext = mainVM
                    };

                    // Подписываемся на событие выхода из MainWindowViewModel
                    mainVM.UserLoggedOut += (s, a) =>
                    {
                        keepRunning = true; // Сигнализируем, что нужно снова показать AuthWindow
                        mainWindow.Close(); // Закрываем MainWindow
                    };

                    // Устанавливаем MainWindow как главное окно для текущей сессии
                    // Это нужно, чтобы приложение не завершилось, если пользователь закроет AuthWindow крестиком в следующий раз
                    // Однако, это может быть сложно, если мы хотим гибко переключаться.
                    // Проще всего управлять ShutdownMode.OnExplicitShutdown и вызывать Current.Shutdown() когда нужно.
                    // Current.MainWindow = mainWindow; // Это можно сделать, но нужно аккуратно управлять

                    mainWindow.ShowDialog(); // Показываем MainWindow как модальное или немодальное

                    // Если mainWindow был закрыт (не через Logout, а, например, крестиком),
                    // то keepRunning останется false, и цикл завершится (приложение закроется).
                    // Если был Logout, keepRunning станет true, и цикл начнется снова.
                }
                else
                {
                    // Аутентификация не удалась (пользователь закрыл AuthWindow или нажал отмену)
                    // keepRunning остается false, приложение завершится после выхода из цикла.
                }
            }

            // Если вышли из цикла while, значит, пора завершать приложение
            Current.Shutdown();
        }
    }
}