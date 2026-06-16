using System;
using System.Collections.ObjectModel;
using System.Windows;
using GasStationIS.Core;
using GasStationIS.Models;
using GasStationIS.Services;

namespace GasStationIS.ViewModels
{
    public class FuelViewModel : BaseViewModel
    {
        private FuelType _selectedFuel;
        private string   _newPriceText;
        private string   _statusMessage;

        public ObservableCollection<FuelType> Fuels { get; } = new ObservableCollection<FuelType>();

        public FuelType SelectedFuel
        {
            get => _selectedFuel;
            set
            {
                SetProperty(ref _selectedFuel, value);
                if (value != null)
                    NewPriceText = value.PricePerLiter.ToString("F2");
            }
        }

        public string NewPriceText
        {
            get => _newPriceText;
            set => SetProperty(ref _newPriceText, value);
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set => SetProperty(ref _statusMessage, value);
        }

        public RelayCommand RefreshCommand    { get; }
        public RelayCommand SavePriceCommand  { get; }

        public FuelViewModel()
        {
            RefreshCommand   = new RelayCommand(_ => Load());
            SavePriceCommand = new RelayCommand(_ => SavePrice(), _ => SelectedFuel != null);
            Load();
        }

        public void Load()
        {
            try
            {
                Fuels.Clear();
                foreach (var f in FuelService.GetAll())
                    Fuels.Add(f);
                StatusMessage = null;
            }
            catch (Exception ex)
            {
                Logger.Error("Ошибка загрузки топлива", ex);
                StatusMessage = "Ошибка загрузки данных.";
            }
        }

        private void SavePrice()
        {
            if (SelectedFuel == null) return;

            if (!double.TryParse(
                    NewPriceText?.Replace(',', '.'),
                    System.Globalization.NumberStyles.Any,
                    System.Globalization.CultureInfo.InvariantCulture,
                    out var price) || price <= 0)
            {
                StatusMessage = "⚠ Введите корректную цену (> 0).";
                return;
            }

            try
            {
                FuelService.UpdatePrice(SelectedFuel.Id, price);
                StatusMessage = $"✓ Цена «{SelectedFuel.Name}» обновлена: {price:F2} руб/л";
                Load();
            }
            catch (Exception ex)
            {
                Logger.Error("Ошибка обновления цены", ex);
                StatusMessage = "Ошибка сохранения цены.";
            }
        }
    }
}
