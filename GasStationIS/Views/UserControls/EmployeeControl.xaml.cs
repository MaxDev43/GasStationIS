using System.Windows.Controls;
using GasStationIS.ViewModels;

namespace GasStationIS.Views.UserControls
{
    public partial class EmployeeControl : UserControl
    {
        private EmployeeViewModel _vm => (EmployeeViewModel)DataContext;

        public EmployeeControl() => InitializeComponent();

        // PasswordBox не поддерживает двухстороннее связывание по соображениям безопасности,
        // поэтому передаём пароль вручную через обработчик события.
        private void NewPwdBox_PasswordChanged(object sender, System.Windows.RoutedEventArgs e)
        {
            if (_vm != null)
                _vm.NewPassword = NewPwdBox.Password;
        }
    }
}
