using System.Windows;
using GasStationIS.Core;
using GasStationIS.Services;

namespace GasStationIS.ViewModels
{
    public class LoginViewModel : BaseViewModel
    {
        private string _login;
        private string _errorMessage;
        private bool   _isBusy;

        public string Login
        {
            get => _login;
            set => SetProperty(ref _login, value);
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value);
        }

        public bool IsBusy
        {
            get => _isBusy;
            set => SetProperty(ref _isBusy, value);
        }

        // Password приходит из View через SecureString → plain text в MinSec сценарии
        public RelayCommand LoginCommand { get; }

        public LoginViewModel()
        {
            LoginCommand = new RelayCommand(
                param => TryLogin(param as string),
                param => !IsBusy && !string.IsNullOrWhiteSpace(Login));
        }

        private void TryLogin(string password)
        {
            ErrorMessage = null;

            if (string.IsNullOrWhiteSpace(Login))
            {
                ErrorMessage = "Введите логин.";
                return;
            }
            if (string.IsNullOrEmpty(password))
            {
                ErrorMessage = "Введите пароль.";
                return;
            }

            IsBusy = true;
            try
            {
                var employee = AuthService.Login(Login.Trim(), password);
                if (employee == null)
                {
                    ErrorMessage = "Неверный логин или пароль.";
                    Logger.Warning($"Неудачная попытка входа: логин='{Login}'");
                    return;
                }

                SessionManager.Login(employee);
                LoginSucceeded?.Invoke();
            }
            catch (System.Exception ex)
            {
                Logger.Error("Ошибка при попытке входа", ex);
                ErrorMessage = "Ошибка подключения к базе данных.";
            }
            finally
            {
                IsBusy = false;
            }
        }

        public System.Action LoginSucceeded { get; set; }
    }
}
