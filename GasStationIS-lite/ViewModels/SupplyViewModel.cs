using System;
using System.Collections.ObjectModel;
using GasStationIS.Core;
using GasStationIS.Models;
using GasStationIS.Services;

namespace GasStationIS.ViewModels
{
    public class SupplyViewModel : BaseViewModel
    {
        public ObservableCollection<FuelSupply> Supplies { get; } = new ObservableCollection<FuelSupply>();
        public ObservableCollection<FuelType> FuelTypes { get; } = new ObservableCollection<FuelType>();

        private string _supplierName;
        private FuelType _selectedFuelType;
        private string _quantityText;
        private string _priceText;
        private string _supplyDate;
        private string _statusMessage;
        private FuelSupply _selectedSupply;

        public string SupplierName { get => _supplierName; set => SetProperty(ref _supplierName, value); }
        public FuelType SelectedFuelType { get => _selectedFuelType; set { SetProperty(ref _selectedFuelType, value); OnPropertyChanged(nameof(CanAdd)); } }
        public string QuantityText { get => _quantityText; set { SetProperty(ref _quantityText, value); OnPropertyChanged(nameof(CalcTotal)); } }
        public string PriceText { get => _priceText; set { SetProperty(ref _priceText, value); OnPropertyChanged(nameof(CalcTotal)); } }
        public string SupplyDate { get => _supplyDate; set => SetProperty(ref _supplyDate, value); }
        public string StatusMessage { get => _statusMessage; set => SetProperty(ref _statusMessage, value); }
        public FuelSupply SelectedSupply { get => _selectedSupply; set => SetProperty(ref _selectedSupply, value); }

        // Предварительная сумма: объём × цена
        public string CalcTotal
        {
            get
            {
                if (double.TryParse(QuantityText?.Replace(',', '.'), System.Globalization.NumberStyles.Any,
                        System.Globalization.CultureInfo.InvariantCulture, out var q) &&
                    double.TryParse(PriceText?.Replace(',', '.'), System.Globalization.NumberStyles.Any,
                        System.Globalization.CultureInfo.InvariantCulture, out var p))
                    return $"{q * p:N2} руб.";
                return "—";
            }
        }

        public bool CanAdd => !string.IsNullOrWhiteSpace(SupplierName) && SelectedFuelType != null;

        public RelayCommand RefreshCommand { get; }
        public RelayCommand AddCommand { get; }
        public RelayCommand DeleteCommand { get; }

        public SupplyViewModel()
        {
            RefreshCommand = new RelayCommand(_ => Load());
            AddCommand = new RelayCommand(_ => AddSupply(), _ => CanAdd);
            DeleteCommand = new RelayCommand(_ => DeleteSupply(), _ => SelectedSupply != null && SessionManager.IsAdmin);

            SupplyDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm");
            Load();
        }

        public void Load()
        {
            try
            {
                Supplies.Clear();
                foreach (var s in SupplyService.GetAll()) Supplies.Add(s);

                FuelTypes.Clear();
                foreach (var f in FuelService.GetAll()) FuelTypes.Add(f);

                StatusMessage = null;
            }
            catch (Exception ex)
            {
                Logger.Error("Ошибка загрузки поставок", ex);
                StatusMessage = "Ошибка загрузки данных.";
            }
        }

        private void AddSupply()
        {
            if (!double.TryParse(QuantityText?.Replace(',', '.'), System.Globalization.NumberStyles.Any,
                    System.Globalization.CultureInfo.InvariantCulture, out var qty) || qty <= 0)
            { StatusMessage = "⚠ Введите корректный объём (> 0)."; return; }

            if (!double.TryParse(PriceText?.Replace(',', '.'), System.Globalization.NumberStyles.Any,
                    System.Globalization.CultureInfo.InvariantCulture, out var price) || price <= 0)
            { StatusMessage = "⚠ Введите корректную цену (> 0)."; return; }

            try
            {
                SupplyService.Add(new FuelSupply
                {
                    SupplierName = SupplierName.Trim(),
                    FuelTypeId = SelectedFuelType.Id,
                    FuelTypeName = SelectedFuelType.Name,
                    QuantityLiters = qty,
                    PricePerLiter = price,
                    SupplyDate = SupplyDate,
                    EmployeeId = SessionManager.CurrentEmployee?.Id,
                });

                StatusMessage = $"✓ Поставка добавлена: {qty} л {SelectedFuelType.Name}";
                ClearForm();
                Load();
            }
            catch (Exception ex)
            {
                Logger.Error("Ошибка добавления поставки", ex);
                StatusMessage = "Ошибка сохранения.";
            }
        }

        private void DeleteSupply()
        {
            if (SelectedSupply == null) return;
            try
            {
                SupplyService.Delete(SelectedSupply.Id);
                StatusMessage = "✓ Запись удалена.";
                Load();
            }
            catch (Exception ex)
            {
                Logger.Error("Ошибка удаления поставки", ex);
                StatusMessage = "Ошибка удаления.";
            }
        }

        private void ClearForm()
        {
            SupplierName = QuantityText = PriceText = null;
            SelectedFuelType = null;
            SupplyDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm");
        }
    }
}