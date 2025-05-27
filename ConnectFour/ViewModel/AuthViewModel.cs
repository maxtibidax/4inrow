// ConnectFour/ViewModel/AuthViewModel.cs
using ConnectFour.Services;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;

namespace ConnectFour.ViewModel
{
    public class AuthViewModel : INotifyPropertyChanged
    {
        private UserService _userService;
        private string _username;
        private string _message;

        public string Username
        {
            get => _username;
            set { _username = value; OnPropertyChanged(); }
        }

        public string Message
        {
            get => _message;
            set { _message = value; OnPropertyChanged(); }
        }

        public RelayCommand LoginCommand { get; private set; }
        public RelayCommand RegisterCommand { get; private set; }

        // Событие для сигнализации об успешном входе
        public event System.EventHandler LoginSuccess;

        public AuthViewModel()
        {
            _userService = new UserService();
            LoginCommand = new RelayCommand(Login, CanLoginOrRegister);
            RegisterCommand = new RelayCommand(Register, CanLoginOrRegister);
        }

        private bool CanLoginOrRegister(object parameter)
        {
            var passwordBox = parameter as PasswordBox;
            return !string.IsNullOrWhiteSpace(Username) &&
                   passwordBox != null &&
                   !string.IsNullOrWhiteSpace(passwordBox.Password);
        }

        private void Login(object parameter)
        {
            var passwordBox = parameter as PasswordBox;
            if (passwordBox == null)
            {
                Message = "Ошибка при получении пароля.";
                return;
            }

            try
            {
                var user = _userService.AuthenticateUser(Username, passwordBox.Password);
                if (user != null)
                {
                    Message = "Вход успешен!";

                    // Небольшая задержка для отображения сообщения
                    System.Threading.Tasks.Task.Delay(500).ContinueWith(t =>
                    {
                        // Вызываем событие в UI потоке
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            LoginSuccess?.Invoke(this, System.EventArgs.Empty);
                        });
                    });
                }
                else
                {
                    Message = "Неверный логин или пароль.";
                }
            }
            catch (System.Exception ex)
            {
                Message = $"Ошибка при входе: {ex.Message}";
            }
        }

        private void Register(object parameter)
        {
            var passwordBox = parameter as PasswordBox;
            if (passwordBox == null)
            {
                Message = "Ошибка при получении пароля.";
                return;
            }

            try
            {
                if (_userService.RegisterUser(Username, passwordBox.Password))
                {
                    Message = "Регистрация успешна! Теперь можете войти.";
                    // Очищаем поля после успешной регистрации
                    passwordBox.Clear();
                }
                else
                {
                    Message = "Ошибка регистрации. Возможно, пользователь уже существует или данные некорректны.";
                }
            }
            catch (System.Exception ex)
            {
                Message = $"Ошибка при регистрации: {ex.Message}";
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            // Обновляем состояние команд при изменении свойств
            LoginCommand?.RaiseCanExecuteChanged();
            RegisterCommand?.RaiseCanExecuteChanged();
        }
    }
}