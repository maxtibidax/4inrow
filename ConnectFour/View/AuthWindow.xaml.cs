// ConnectFour/View/AuthWindow.xaml.cs
using System.Windows;

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
        private void Window_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Escape)
            {
                this.DialogResult = false;
                this.Close();
            }
        }

        // Обработчик события Loaded для фокуса на поле Username
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Можно добавить логику для установки фокуса
        }
    }
}