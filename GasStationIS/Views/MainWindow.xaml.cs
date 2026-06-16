using System.Windows;
using System.Windows.Input;
using GasStationIS.ViewModels;
using GasStationIS.Views.UserControls;

namespace GasStationIS.Views
{
    public partial class MainWindow : Window
    {
        private MainViewModel _vm => (MainViewModel)DataContext;

        // Кэш UserControl-ов — создаём один раз, не пересоздаём при каждом клике
        private DashboardControl  _dashboard;
        private FuelControl       _fuel;
        private SupplyControl     _supply;
        private SalesControl      _sales;
        private ShopControl       _shop;
        private CustomerControl   _customers;
        private EmployeeControl   _employees;
        private ReportsControl    _reports;
        private SettingsControl   _settings;

        public MainWindow()
        {
            InitializeComponent();

            _vm.LogoutRequested = DoLogout;

            // Реагируем на смену раздела
            _vm.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(MainViewModel.CurrentSection))
                    NavigateTo(_vm.CurrentSection);
            };

            // Стартовая страница
            NavigateTo("dashboard");
        }

        private void NavigateTo(string section)
        {
            switch (section)
            {
                case "dashboard":
                    _dashboard ??= new DashboardControl();
                    (_dashboard.DataContext as DashboardViewModel)?.Load();
                    ContentArea.Content = _dashboard;
                    break;

                case "fuel":
                    _fuel ??= new FuelControl();
                    (_fuel.DataContext as FuelViewModel)?.Load();
                    ContentArea.Content = _fuel;
                    break;

                case "supply":
                    _supply ??= new SupplyControl();
                    (_supply.DataContext as SupplyViewModel)?.Load();
                    ContentArea.Content = _supply;
                    break;

                case "sales":
                    _sales ??= new SalesControl();
                    (_sales.DataContext as SalesViewModel)?.Load();
                    ContentArea.Content = _sales;
                    break;

                case "shop":
                    _shop ??= new ShopControl();
                    (_shop.DataContext as ShopViewModel)?.Load();
                    ContentArea.Content = _shop;
                    break;

                case "customers":
                    _customers ??= new CustomerControl();
                    (_customers.DataContext as CustomerViewModel)?.Load();
                    ContentArea.Content = _customers;
                    break;

                case "employees":
                    _employees ??= new EmployeeControl();
                    (_employees.DataContext as EmployeeViewModel)?.Load();
                    ContentArea.Content = _employees;
                    break;

                case "reports":
                    _reports ??= new ReportsControl();
                    ContentArea.Content = _reports;
                    break;

                case "settings":
                    _settings ??= new SettingsControl();
                    (_settings.DataContext as SettingsViewModel)?.Load();
                    ContentArea.Content = _settings;
                    break;
            }
        }

        private void DoLogout()
        {
            Core.SessionManager.Logout();
            var login = new LoginWindow();
            login.Show();
            Close();
        }

        private void TitleBar_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                DragMove();
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
            => WindowState = WindowState.Minimized;

        private void CloseButton_Click(object sender, RoutedEventArgs e)
            => Application.Current.Shutdown();
    }
}
