using System.Windows;
using GasStationIS.Core;

namespace GasStationIS.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        private string _currentSection = "dashboard";
        private string _stationName;
        private string _currentUserName;
        private string _currentUserRole;

        public string CurrentSection
        {
            get => _currentSection;
            set => SetProperty(ref _currentSection, value);
        }

        public string StationName
        {
            get => _stationName;
            set => SetProperty(ref _stationName, value);
        }

        public string CurrentUserName
        {
            get => _currentUserName;
            set => SetProperty(ref _currentUserName, value);
        }

        public string CurrentUserRole
        {
            get => _currentUserRole;
            set => SetProperty(ref _currentUserRole, value);
        }

        // Видимость разделов по роли — вычисляется один раз после входа
        // (MainWindow создаётся только после успешной авторизации, значение стабильно)
        public Visibility AdminOnlyVisibility { get; } =
            SessionManager.IsAdmin ? Visibility.Visible : Visibility.Collapsed;

        // Команды навигации
        public RelayCommand NavDashboardCommand  { get; }
        public RelayCommand NavFuelCommand       { get; }
        public RelayCommand NavSupplyCommand     { get; }
        public RelayCommand NavSalesCommand      { get; }
        public RelayCommand NavCustomerCommand   { get; }
        public RelayCommand NavEmployeeCommand   { get; }
        public RelayCommand NavReportsCommand    { get; }
        public RelayCommand NavSettingsCommand   { get; }
        public RelayCommand LogoutCommand        { get; }

        public System.Action LogoutRequested { get; set; }

        public MainViewModel()
        {
            NavDashboardCommand = new RelayCommand(_ => CurrentSection = "dashboard");
            NavFuelCommand      = new RelayCommand(_ => CurrentSection = "fuel");
            NavSupplyCommand    = new RelayCommand(_ => CurrentSection = "supply");
            NavSalesCommand     = new RelayCommand(_ => CurrentSection = "sales");
            NavCustomerCommand  = new RelayCommand(_ => CurrentSection = "customers");
            NavEmployeeCommand  = new RelayCommand(
                _ => CurrentSection = "employees",
                _ => SessionManager.IsAdmin);
            NavReportsCommand   = new RelayCommand(_ => CurrentSection = "reports");
            NavSettingsCommand  = new RelayCommand(
                _ => CurrentSection = "settings",
                _ => SessionManager.IsAdmin);
            LogoutCommand       = new RelayCommand(_ => LogoutRequested?.Invoke());

            LoadUserInfo();
        }

        private void LoadUserInfo()
        {
            var emp = SessionManager.CurrentEmployee;
            if (emp != null)
            {
                CurrentUserName = emp.FullName;
                CurrentUserRole = emp.Role;
            }

            try
            {
                var info = Services.StationInfoService.Get();
                StationName = info?.Name ?? "АЗС";
            }
            catch
            {
                StationName = "АЗС";
            }
        }
    }
}
