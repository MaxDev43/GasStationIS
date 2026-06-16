using System;
using System.Collections.ObjectModel;
using GasStationIS.Core;
using GasStationIS.Models;
using GasStationIS.Services;

namespace GasStationIS.ViewModels
{
    // ════════════════════════════════════════════════════════════════════
    //  SalesViewModel
    // ════════════════════════════════════════════════════════════════════
    public class SalesViewModel : BaseViewModel
    {
        public ObservableCollection<FuelSale> Sales     { get; } = new ObservableCollection<FuelSale>();
        public ObservableCollection<FuelType> FuelTypes { get; } = new ObservableCollection<FuelType>();
        public ObservableCollection<Customer> Customers { get; } = new ObservableCollection<Customer>();

        private FuelType _selectedFuelType;
        private Customer _selectedCustomer;
        private string   _litersText;
        private string   _priceText;
        private string   _paymentMethod = "Наличные";
        private string   _pumpText;
        private string   _notes;
        private string   _statusMessage;
        private FuelSale _selectedSale;
        private DateTime _filterFrom = DateTime.Today.AddDays(-30);
        private DateTime _filterTo   = DateTime.Today;

        public FuelType  SelectedFuelType  { get => _selectedFuelType; set { SetProperty(ref _selectedFuelType, value); if (value!=null) PriceText = value.PricePerLiter.ToString("F2"); OnPropertyChanged(nameof(CalcTotal)); } }
        public Customer  SelectedCustomer  { get => _selectedCustomer; set => SetProperty(ref _selectedCustomer, value); }
        public string    LitersText        { get => _litersText;       set { SetProperty(ref _litersText, value); OnPropertyChanged(nameof(CalcTotal)); } }
        public string    PriceText         { get => _priceText;        set { SetProperty(ref _priceText, value); OnPropertyChanged(nameof(CalcTotal)); } }
        public string    PaymentMethod     { get => _paymentMethod;    set => SetProperty(ref _paymentMethod, value); }
        public string    PumpText          { get => _pumpText;         set => SetProperty(ref _pumpText, value); }
        public string    Notes             { get => _notes;            set => SetProperty(ref _notes, value); }
        public string    StatusMessage     { get => _statusMessage;    set => SetProperty(ref _statusMessage, value); }
        public FuelSale  SelectedSale      { get => _selectedSale;     set => SetProperty(ref _selectedSale, value); }
        public DateTime  FilterFrom        { get => _filterFrom;       set => SetProperty(ref _filterFrom, value); }
        public DateTime  FilterTo          { get => _filterTo;         set => SetProperty(ref _filterTo, value); }

        public string CalcTotal
        {
            get
            {
                if (double.TryParse(LitersText?.Replace(',','.'), System.Globalization.NumberStyles.Any,
                        System.Globalization.CultureInfo.InvariantCulture, out var l) &&
                    double.TryParse(PriceText?.Replace(',','.'), System.Globalization.NumberStyles.Any,
                        System.Globalization.CultureInfo.InvariantCulture, out var p))
                    return $"{l * p:N2} руб.";
                return "—";
            }
        }

        public RelayCommand RefreshCommand { get; }
        public RelayCommand AddCommand     { get; }
        public RelayCommand FilterCommand  { get; }
        public RelayCommand DeleteCommand  { get; }

        public SalesViewModel()
        {
            RefreshCommand = new RelayCommand(_ => Load());
            AddCommand     = new RelayCommand(_ => AddSale(), _ => SelectedFuelType != null);
            FilterCommand  = new RelayCommand(_ => LoadFiltered());
            DeleteCommand  = new RelayCommand(_ => DeleteSale(), _ => SelectedSale != null && SessionManager.IsAdmin);
            Load();
        }

        public void Load()
        {
            try
            {
                FuelTypes.Clear();
                foreach (var f in FuelService.GetAll()) FuelTypes.Add(f);
                Customers.Clear();
                Customers.Add(new Customer { Id = 0, FullName = "(без клиента)" });
                foreach (var c in CustomerService.GetAll()) Customers.Add(c);
                LoadFiltered();
            }
            catch (Exception ex) { Logger.Error("Ошибка загрузки продаж", ex); StatusMessage = "Ошибка загрузки."; }
        }

        private void LoadFiltered()
        {
            Sales.Clear();
            var list = SaleService.GetByPeriod(FilterFrom, FilterTo);
            foreach (var s in list) Sales.Add(s);
        }

        private void AddSale()
        {
            if (!double.TryParse(LitersText?.Replace(',','.'), System.Globalization.NumberStyles.Any,
                    System.Globalization.CultureInfo.InvariantCulture, out var liters) || liters <= 0)
            { StatusMessage = "⚠ Введите корректный объём (> 0)."; return; }

            if (!double.TryParse(PriceText?.Replace(',','.'), System.Globalization.NumberStyles.Any,
                    System.Globalization.CultureInfo.InvariantCulture, out var price) || price <= 0)
            { StatusMessage = "⚠ Введите корректную цену."; return; }

            int? custId = SelectedCustomer?.Id > 0 ? SelectedCustomer.Id : (int?)null;
            int.TryParse(PumpText, out var pump);

            try
            {
                bool ok = SaleService.Add(new FuelSale
                {
                    FuelTypeId    = SelectedFuelType.Id,
                    FuelTypeName  = SelectedFuelType.Name,
                    Liters        = liters,
                    PricePerLiter = price,
                    SaleDatetime  = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    PaymentMethod = PaymentMethod ?? "Наличные",
                    EmployeeId    = SessionManager.CurrentEmployee?.Id,
                    CustomerId    = custId,
                    PumpNumber    = pump > 0 ? (int?)pump : null,
                    Notes         = Notes?.Trim(),
                });

                if (!ok) { StatusMessage = "⚠ Недостаточно топлива в резервуаре!"; return; }

                StatusMessage = $"✓ Продажа зарегистрирована: {liters} л × {price:F2} = {liters*price:N2} руб.";
                LitersText = Notes = null;
                Load();
            }
            catch (Exception ex) { Logger.Error("Ошибка добавления продажи", ex); StatusMessage = "Ошибка сохранения."; }
        }

        private void DeleteSale()
        {
            if (SelectedSale == null) return;
            try { SaleService.Delete(SelectedSale.Id); StatusMessage = "✓ Запись удалена."; Load(); }
            catch (Exception ex) { Logger.Error("Ошибка удаления", ex); }
        }
    }

    // ════════════════════════════════════════════════════════════════════
    //  ShopViewModel
    // ════════════════════════════════════════════════════════════════════
    public class ShopViewModel : BaseViewModel
    {
        public ObservableCollection<ShopProduct> Products { get; } = new ObservableCollection<ShopProduct>();
        public ObservableCollection<ShopSale>    Sales    { get; } = new ObservableCollection<ShopSale>();
        public ObservableCollection<ShopSaleItem> CartItems { get; } = new ObservableCollection<ShopSaleItem>();

        private ShopProduct _selectedProduct;
        private ShopSale    _selectedSale;
        private string      _newName, _newCategory, _newPriceText, _newStockText, _newBarcode;
        private int         _cartQty = 1;
        private string      _paymentMethod = "Наличные";
        private string      _statusMessage;

        public ShopProduct SelectedProduct  { get => _selectedProduct; set => SetProperty(ref _selectedProduct, value); }
        public ShopSale    SelectedSale     { get => _selectedSale;    set => SetProperty(ref _selectedSale, value); }
        public string      NewName          { get => _newName;         set => SetProperty(ref _newName, value); }
        public string      NewCategory      { get => _newCategory;     set => SetProperty(ref _newCategory, value); }
        public string      NewPriceText     { get => _newPriceText;    set => SetProperty(ref _newPriceText, value); }
        public string      NewStockText     { get => _newStockText;    set => SetProperty(ref _newStockText, value); }
        public string      NewBarcode       { get => _newBarcode;      set => SetProperty(ref _newBarcode, value); }
        public int         CartQty          { get => _cartQty;         set => SetProperty(ref _cartQty, value); }
        public string      PaymentMethod    { get => _paymentMethod;   set => SetProperty(ref _paymentMethod, value); }
        public string      StatusMessage    { get => _statusMessage;   set => SetProperty(ref _statusMessage, value); }

        public double CartTotal => GetCartTotal();

        public RelayCommand RefreshCommand      { get; }
        public RelayCommand AddProductCommand   { get; }
        public RelayCommand DeleteProductCommand{ get; }
        public RelayCommand AddToCartCommand    { get; }
        public RelayCommand RemoveFromCartCommand{ get; }
        public RelayCommand CheckoutCommand     { get; }

        public ShopViewModel()
        {
            RefreshCommand       = new RelayCommand(_ => Load());
            AddProductCommand    = new RelayCommand(_ => AddProduct());
            DeleteProductCommand = new RelayCommand(_ => DeleteProduct(), _ => SelectedProduct != null && SessionManager.IsAdmin);
            AddToCartCommand     = new RelayCommand(_ => AddToCart(), _ => SelectedProduct != null);
            RemoveFromCartCommand= new RelayCommand(p => RemoveFromCart(p as ShopSaleItem), _ => CartItems.Count > 0);
            CheckoutCommand      = new RelayCommand(_ => Checkout(), _ => CartItems.Count > 0);
            Load();
        }

        public void Load()
        {
            Products.Clear();
            foreach (var p in ShopService.GetAllProducts()) Products.Add(p);
            Sales.Clear();
            foreach (var s in ShopService.GetAllSales()) Sales.Add(s);
        }

        private void AddProduct()
        {
            if (string.IsNullOrWhiteSpace(NewName)) { StatusMessage = "⚠ Введите название."; return; }
            if (!double.TryParse(NewPriceText?.Replace(',','.'), System.Globalization.NumberStyles.Any,
                    System.Globalization.CultureInfo.InvariantCulture, out var price) || price < 0)
            { StatusMessage = "⚠ Введите корректную цену."; return; }
            if (!int.TryParse(NewStockText, out var stock) || stock < 0)
            { StatusMessage = "⚠ Введите корректный остаток."; return; }

            try
            {
                ShopService.AddProduct(new ShopProduct { Name = NewName.Trim(), Category = NewCategory?.Trim(), Price = price, Stock = stock, Barcode = NewBarcode?.Trim() });
                StatusMessage = "✓ Товар добавлен.";
                NewName = NewCategory = NewPriceText = NewStockText = NewBarcode = null;
                Load();
            }
            catch (Exception ex) { Logger.Error("Ошибка добавления товара", ex); StatusMessage = "Ошибка."; }
        }

        private void DeleteProduct()
        {
            try { ShopService.DeleteProduct(SelectedProduct.Id); StatusMessage = "✓ Товар удалён."; Load(); }
            catch (Exception ex) { Logger.Error("Ошибка удаления товара", ex); }
        }

        private void AddToCart()
        {
            if (SelectedProduct == null || CartQty <= 0) return;
            if (SelectedProduct.Stock < CartQty) { StatusMessage = $"⚠ На складе только {SelectedProduct.Stock} шт."; return; }
            CartItems.Add(new ShopSaleItem { ProductId = SelectedProduct.Id, ProductName = SelectedProduct.Name, Price = SelectedProduct.Price, Quantity = CartQty });
            OnPropertyChanged(nameof(CartTotal));
            StatusMessage = null;
        }

        private void RemoveFromCart(ShopSaleItem item)
        {
            if (item != null) { CartItems.Remove(item); OnPropertyChanged(nameof(CartTotal)); }
        }

        private void Checkout()
        {
            if (CartItems.Count == 0) return;
            try
            {
                var sale = new ShopSale { SaleDatetime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), TotalAmount = CartTotal, PaymentMethod = PaymentMethod, EmployeeId = SessionManager.CurrentEmployee?.Id };
                ShopService.AddSale(sale, new System.Collections.Generic.List<ShopSaleItem>(CartItems));
                StatusMessage = $"✓ Чек проведён: {CartTotal:N2} руб.";
                CartItems.Clear();
                OnPropertyChanged(nameof(CartTotal));
                Load();
            }
            catch (Exception ex) { Logger.Error("Ошибка проведения чека", ex); StatusMessage = "Ошибка."; }
        }

        private double GetCartTotal()
        {
            double t = 0;
            foreach (var i in CartItems) t += i.Total;
            return t;
        }
    }

    // ════════════════════════════════════════════════════════════════════
    //  CustomerViewModel
    // ════════════════════════════════════════════════════════════════════
    public class CustomerViewModel : BaseViewModel
    {
        public ObservableCollection<Customer> Customers { get; } = new ObservableCollection<Customer>();

        private Customer _selectedCustomer;
        private string _fullName, _phone, _email, _carNumber, _statusMessage;

        public Customer SelectedCustomer { get => _selectedCustomer; set { SetProperty(ref _selectedCustomer, value); FillForm(); } }
        public string   FullName         { get => _fullName;         set => SetProperty(ref _fullName, value); }
        public string   Phone            { get => _phone;            set => SetProperty(ref _phone, value); }
        public string   Email            { get => _email;            set => SetProperty(ref _email, value); }
        public string   CarNumber        { get => _carNumber;        set => SetProperty(ref _carNumber, value); }
        public string   StatusMessage    { get => _statusMessage;    set => SetProperty(ref _statusMessage, value); }

        public RelayCommand RefreshCommand { get; }
        public RelayCommand AddCommand     { get; }
        public RelayCommand UpdateCommand  { get; }
        public RelayCommand DeleteCommand  { get; }

        public CustomerViewModel()
        {
            RefreshCommand = new RelayCommand(_ => Load());
            AddCommand     = new RelayCommand(_ => AddCustomer());
            UpdateCommand  = new RelayCommand(_ => UpdateCustomer(), _ => SelectedCustomer != null);
            DeleteCommand  = new RelayCommand(_ => DeleteCustomer(), _ => SelectedCustomer != null && SessionManager.IsAdmin);
            Load();
        }

        public void Load()
        {
            Customers.Clear();
            foreach (var c in CustomerService.GetAll()) Customers.Add(c);
        }

        private void FillForm()
        {
            if (SelectedCustomer == null) return;
            FullName = SelectedCustomer.FullName;
            Phone    = SelectedCustomer.Phone;
            Email    = SelectedCustomer.Email;
            CarNumber= SelectedCustomer.CarNumber;
        }

        private void AddCustomer()
        {
            if (string.IsNullOrWhiteSpace(FullName)) { StatusMessage = "⚠ Введите ФИО."; return; }
            try
            {
                CustomerService.Add(new Customer { FullName = FullName.Trim(), Phone = Phone?.Trim(), Email = Email?.Trim(), CarNumber = CarNumber?.Trim() });
                StatusMessage = "✓ Клиент добавлен.";
                ClearForm(); Load();
            }
            catch (Exception ex) { Logger.Error("Ошибка добавления клиента", ex); StatusMessage = "Ошибка."; }
        }

        private void UpdateCustomer()
        {
            if (SelectedCustomer == null) return;
            try
            {
                CustomerService.Update(new Customer { Id = SelectedCustomer.Id, FullName = FullName?.Trim() ?? "", Phone = Phone?.Trim(), Email = Email?.Trim(), CarNumber = CarNumber?.Trim() });
                StatusMessage = "✓ Данные обновлены.";
                Load();
            }
            catch (Exception ex) { Logger.Error("Ошибка обновления клиента", ex); }
        }

        private void DeleteCustomer()
        {
            if (SelectedCustomer == null) return;
            try { CustomerService.Delete(SelectedCustomer.Id); StatusMessage = "✓ Клиент удалён."; ClearForm(); Load(); }
            catch (Exception ex) { Logger.Error("Ошибка удаления клиента", ex); }
        }

        private void ClearForm() { FullName = Phone = Email = CarNumber = null; SelectedCustomer = null; }
    }

    // ════════════════════════════════════════════════════════════════════
    //  EmployeeViewModel
    // ════════════════════════════════════════════════════════════════════
    public class EmployeeViewModel : BaseViewModel
    {
        public ObservableCollection<Employee> Employees { get; } = new ObservableCollection<Employee>();

        private Employee _selected;
        private string _fullName, _role = "Оператор", _login, _phone, _hireDate, _newPassword, _statusMessage;

        public Employee SelectedEmployee  { get => _selected;       set { SetProperty(ref _selected, value); FillForm(); } }
        public string   FullName          { get => _fullName;       set => SetProperty(ref _fullName, value); }
        public string   Role              { get => _role;           set => SetProperty(ref _role, value); }
        public string   Login             { get => _login;          set => SetProperty(ref _login, value); }
        public string   Phone             { get => _phone;          set => SetProperty(ref _phone, value); }
        public string   HireDate          { get => _hireDate;       set => SetProperty(ref _hireDate, value); }
        public string   NewPassword       { get => _newPassword;    set => SetProperty(ref _newPassword, value); }
        public string   StatusMessage     { get => _statusMessage;  set => SetProperty(ref _statusMessage, value); }

        public System.Collections.Generic.List<string> Roles { get; } =
            new System.Collections.Generic.List<string> { "Администратор", "Оператор" };

        public RelayCommand RefreshCommand        { get; }
        public RelayCommand AddCommand            { get; }
        public RelayCommand UpdateCommand         { get; }
        public RelayCommand ChangePasswordCommand { get; }
        public RelayCommand DeactivateCommand     { get; }

        public EmployeeViewModel()
        {
            RefreshCommand        = new RelayCommand(_ => Load());
            AddCommand            = new RelayCommand(_ => AddEmployee());
            UpdateCommand         = new RelayCommand(_ => UpdateEmployee(), _ => SelectedEmployee != null);
            ChangePasswordCommand = new RelayCommand(_ => ChangePassword(), _ => SelectedEmployee != null && !string.IsNullOrEmpty(NewPassword));
            DeactivateCommand     = new RelayCommand(_ => ToggleActive(), _ => SelectedEmployee != null);
            HireDate = DateTime.Now.ToString("yyyy-MM-dd");
            Load();
        }

        public void Load()
        {
            Employees.Clear();
            foreach (var e in EmployeeService.GetAll()) Employees.Add(e);
        }

        private void FillForm()
        {
            if (SelectedEmployee == null) return;
            FullName = SelectedEmployee.FullName;
            Role     = SelectedEmployee.Role;
            Login    = SelectedEmployee.Login;
            Phone    = SelectedEmployee.Phone;
            HireDate = SelectedEmployee.HireDate;
        }

        private void AddEmployee()
        {
            if (string.IsNullOrWhiteSpace(FullName) || string.IsNullOrWhiteSpace(Login) || string.IsNullOrWhiteSpace(NewPassword))
            { StatusMessage = "⚠ Заполните ФИО, логин и пароль."; return; }
            try
            {
                EmployeeService.Add(new Employee { FullName = FullName.Trim(), Role = Role, Login = Login.Trim(), Phone = Phone?.Trim(), HireDate = HireDate }, NewPassword);
                StatusMessage = "✓ Сотрудник добавлен.";
                ClearForm(); Load();
            }
            catch (Exception ex) { Logger.Error("Ошибка добавления сотрудника", ex); StatusMessage = $"Ошибка: {ex.Message}"; }
        }

        private void UpdateEmployee()
        {
            if (SelectedEmployee == null) return;
            try
            {
                EmployeeService.Update(new Employee { Id = SelectedEmployee.Id, FullName = FullName?.Trim() ?? "", Role = Role, Login = Login?.Trim() ?? "", Phone = Phone?.Trim() });
                StatusMessage = "✓ Данные обновлены."; Load();
            }
            catch (Exception ex) { Logger.Error("Ошибка обновления сотрудника", ex); StatusMessage = $"Ошибка: {ex.Message}"; }
        }

        private void ChangePassword()
        {
            if (SelectedEmployee == null || string.IsNullOrWhiteSpace(NewPassword)) return;
            try { EmployeeService.ChangePassword(SelectedEmployee.Id, NewPassword); StatusMessage = "✓ Пароль изменён."; NewPassword = null; }
            catch (Exception ex) { Logger.Error("Ошибка смены пароля", ex); }
        }

        private void ToggleActive()
        {
            if (SelectedEmployee == null) return;
            try { EmployeeService.SetActive(SelectedEmployee.Id, !SelectedEmployee.IsActive); StatusMessage = "✓ Статус изменён."; Load(); }
            catch (Exception ex) { Logger.Error("Ошибка изменения статуса", ex); }
        }

        private void ClearForm() { FullName = Login = Phone = NewPassword = null; Role = "Оператор"; SelectedEmployee = null; HireDate = DateTime.Now.ToString("yyyy-MM-dd"); }
    }

    // ════════════════════════════════════════════════════════════════════
    //  ReportsViewModel
    // ════════════════════════════════════════════════════════════════════
    public class ReportsViewModel : BaseViewModel
    {
        public ObservableCollection<ReportEntry> FuelSalesReport { get; } = new ObservableCollection<ReportEntry>();
        public ObservableCollection<ReportEntry> FuelStockReport { get; } = new ObservableCollection<ReportEntry>();
        public ObservableCollection<ReportEntry> DailyRevenue    { get; } = new ObservableCollection<ReportEntry>();

        private DateTime _from = DateTime.Today.AddDays(-30);
        private DateTime _to   = DateTime.Today;
        private double   _fuelRevenue, _shopRevenue, _totalRevenue;
        private long     _fuelCount, _shopCount;
        private string   _statusMessage;

        public DateTime From          { get => _from;         set => SetProperty(ref _from, value); }
        public DateTime To            { get => _to;           set => SetProperty(ref _to, value); }
        public double   FuelRevenue   { get => _fuelRevenue;  set => SetProperty(ref _fuelRevenue, value); }
        public double   ShopRevenue   { get => _shopRevenue;  set => SetProperty(ref _shopRevenue, value); }
        public double   TotalRevenue  { get => _totalRevenue; set => SetProperty(ref _totalRevenue, value); }
        public long     FuelCount     { get => _fuelCount;    set => SetProperty(ref _fuelCount, value); }
        public long     ShopCount     { get => _shopCount;    set => SetProperty(ref _shopCount, value); }
        public string   StatusMessage { get => _statusMessage; set => SetProperty(ref _statusMessage, value); }

        public RelayCommand GenerateCommand { get; }

        public ReportsViewModel()
        {
            GenerateCommand = new RelayCommand(_ => Generate());
            Generate();
        }

        public void Generate()
        {
            try
            {
                FuelSalesReport.Clear();
                foreach (var r in ReportService.GetFuelSalesByType(From, To)) FuelSalesReport.Add(r);
                FuelStockReport.Clear();
                foreach (var r in ReportService.GetFuelStock()) FuelStockReport.Add(r);
                DailyRevenue.Clear();
                foreach (var r in ReportService.GetDailyRevenue(From, To)) DailyRevenue.Add(r);

                var (fr, sr, tot, fc, sc) = ReportService.GetSummary(From, To);
                FuelRevenue = fr; ShopRevenue = sr; TotalRevenue = tot;
                FuelCount = fc; ShopCount = sc;
                StatusMessage = $"Отчёт за период: {From:dd.MM.yyyy} — {To:dd.MM.yyyy}";
            }
            catch (Exception ex) { Logger.Error("Ошибка генерации отчёта", ex); StatusMessage = "Ошибка формирования отчёта."; }
        }
    }

    // ════════════════════════════════════════════════════════════════════
    //  SettingsViewModel
    // ════════════════════════════════════════════════════════════════════
    public class SettingsViewModel : BaseViewModel
    {
        private string _name, _address, _phone, _director, _inn, _license, _statusMessage;

        public string Name          { get => _name;          set => SetProperty(ref _name, value); }
        public string Address       { get => _address;       set => SetProperty(ref _address, value); }
        public string Phone         { get => _phone;         set => SetProperty(ref _phone, value); }
        public string Director      { get => _director;      set => SetProperty(ref _director, value); }
        public string Inn           { get => _inn;           set => SetProperty(ref _inn, value); }
        public string License       { get => _license;       set => SetProperty(ref _license, value); }
        public string StatusMessage { get => _statusMessage; set => SetProperty(ref _statusMessage, value); }
        public string DbPath        => Core.DatabaseService.DbPath;

        public RelayCommand LoadCommand   { get; }
        public RelayCommand SaveCommand   { get; }
        public RelayCommand BackupCommand { get; }

        public SettingsViewModel()
        {
            LoadCommand   = new RelayCommand(_ => Load());
            SaveCommand   = new RelayCommand(_ => Save());
            BackupCommand = new RelayCommand(_ => Backup());
            Load();
        }

        public void Load()
        {
            try
            {
                var info = StationInfoService.Get();
                Name = info.Name; Address = info.Address; Phone = info.Phone;
                Director = info.Director; Inn = info.Inn; License = info.LicenseNumber;
                StatusMessage = null;
            }
            catch (Exception ex) { Logger.Error("Ошибка загрузки настроек", ex); }
        }

        private void Save()
        {
            if (string.IsNullOrWhiteSpace(Name)) { StatusMessage = "⚠ Название обязательно."; return; }
            try
            {
                StationInfoService.Save(new StationInfo { Name = Name?.Trim(), Address = Address?.Trim(), Phone = Phone?.Trim(), Director = Director?.Trim(), Inn = Inn?.Trim(), LicenseNumber = License?.Trim() });
                StatusMessage = "✓ Настройки сохранены.";
            }
            catch (Exception ex) { Logger.Error("Ошибка сохранения настроек", ex); StatusMessage = "Ошибка сохранения."; }
        }

        private void Backup()
        {
            try
            {
                var dest = System.IO.Path.Combine(
                    System.IO.Path.GetDirectoryName(DbPath),
                    $"azs_backup_{DateTime.Now:yyyyMMdd_HHmmss}.db");
                System.IO.File.Copy(DbPath, dest, overwrite: false);
                StatusMessage = $"✓ Резервная копия создана: {System.IO.Path.GetFileName(dest)}";
                Logger.Info($"Бэкап БД: {dest}");
            }
            catch (Exception ex) { Logger.Error("Ошибка резервного копирования", ex); StatusMessage = "Ошибка создания резервной копии."; }
        }
    }
}
