using System.Windows;
using System.Windows.Input;
using GasStationIS.ViewModels;

namespace GasStationIS.Views
{
    public partial class LoginWindow : Window
    {
        // ═══════════════════════════════════════════════════════════════════════
        // АВТОЗАПОЛНЕНИЕ: логин и пароль подставляются при открытии окна.
        // Чтобы ОТКЛЮЧИТЬ автоввод — поставьте false.
        // ═══════════════════════════════════════════════════════════════════════
        private const bool AutoFillCredentials = true;
        private const string AutoFillLogin = "admin";
        private const string AutoFillPassword = "admin123";
        // ═══════════════════════════════════════════════════════════════════════

        private LoginViewModel _vm => (LoginViewModel)DataContext;

        public LoginWindow()
        {
            InitializeComponent();

            _vm.LoginSucceeded = () =>
            {
                var main = new MainWindow();
                main.Show();
                Close();
            };

            // Автозаполнение
            if (AutoFillCredentials)
            {
                _vm.Login = AutoFillLogin;
                PwdBox.Password = AutoFillPassword;
            }

            // Перетаскивание окна без рамки
            MouseDown += (s, e) =>
            {
                if (e.LeftButton == MouseButtonState.Pressed)
                    DragMove();
            };
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            _vm.LoginCommand.Execute(PwdBox.Password);
        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                _vm.LoginCommand.Execute(PwdBox.Password);
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}