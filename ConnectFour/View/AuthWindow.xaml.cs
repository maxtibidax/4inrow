using System.Windows;
using System.Windows.Input;

namespace ConnectFour.View
{
    /// <summary>
    /// Логика взаимодействия для AuthWindow.xaml
    /// </summary>
    public partial class AuthWindow : Window
    {
        public AuthWindow()
        {
            InitializeComponent();
        }

        // Обработчик для закрытия окна по Escape или Alt+F4
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                this.DialogResult = false;
                this.Close();
            }
        }

        // Обработчик события Loaded для фокуса на поле Username
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Устанавливаем фокус на первое поле ввода
            var firstTextBox = this.FindName("UsernameTextBox") as System.Windows.Controls.TextBox;
            firstTextBox?.Focus();
        }

        // Обработчик для перетаскивания окна за заголовок
        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
            {
                this.DragMove();
            }
        }

        // Обработчик для кнопки закрытия
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        // Обработчик для Enter в полях ввода
        private void InputField_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return || e.Key == Key.Enter)
            {
                // Можно добавить логику для автоматического входа при нажатии Enter
                var loginButton = this.FindName("LoginButton") as System.Windows.Controls.Button;
                if (loginButton?.Command?.CanExecute(PasswordBoxAuth) == true)
                {
                    loginButton.Command.Execute(PasswordBoxAuth);
                }
            }
        }
    }
}