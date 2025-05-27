// ConnectFour/ViewModel/AuthViewModel.cs
using ConnectFour.Services;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows; // Для MessageBox и Window
using System.Windows.Controls; // Для PasswordBox

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
            // Простая проверка, можно усложнить
            return !string.IsNullOrWhiteSpace(Username) && (parameter as PasswordBox)?.Password.Length > 0;
        }

        private void Login(object parameter)
        {
            var passwordBox = parameter as PasswordBox;
            if (passwordBox == null) return;

            var user = _userService.AuthenticateUser(Username, passwordBox.Password);
            if (user != null)
            {
                Message = "Вход успешен!";
                // Сигнализируем, что логин успешен
                LoginSuccess?.Invoke(this, System.EventArgs.Empty);
            }
            else
            {
                Message = "Неверный логин или пароль.";
            }
        }

        private void Register(object parameter)
        {
            var passwordBox = parameter as PasswordBox;
            if (passwordBox == null) return;

            if (_userService.RegisterUser(Username, passwordBox.Password))
            {
                Message = "Регистрация успешна! Теперь можете войти.";
            }
            else
            {
                Message = "Ошибка регистрации. Возможно, пользователь уже существует или данные некорректны.";
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            // Обновляем состояние команд при изменении свойств, от которых они зависят
            LoginCommand.RaiseCanExecuteChanged();
            RegisterCommand.RaiseCanExecuteChanged();
        }
    }
}