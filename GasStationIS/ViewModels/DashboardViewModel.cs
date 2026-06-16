using System;
using System.Collections.ObjectModel;
using GasStationIS.Core;
using GasStationIS.Models;
using GasStationIS.Services;

namespace GasStationIS.ViewModels
{
    public class DashboardViewModel : BaseViewModel
    {
        private double _todayFuelRevenue;
        private double _todayShopRevenue;
        private long   _todayFuelCount;
        private long   _activeEmployees;
        private long   _totalCustomers;

        public double TodayFuelRevenue  { get => _todayFuelRevenue;  set => SetProperty(ref _todayFuelRevenue,  value); }
        public double TodayShopRevenue  { get => _todayShopRevenue;  set => SetProperty(ref _todayShopRevenue,  value); }
        public long   TodayFuelCount    { get => _todayFuelCount;    set => SetProperty(ref _todayFuelCount,    value); }
        public long   ActiveEmployees   { get => _activeEmployees;   set => SetProperty(ref _activeEmployees,   value); }
        public long   TotalCustomers    { get => _totalCustomers;    set => SetProperty(ref _totalCustomers,    value); }
        public double TodayTotalRevenue => TodayFuelRevenue + TodayShopRevenue;

        public ObservableCollection<FuelType>  FuelLevels    { get; } = new ObservableCollection<FuelType>();
        public ObservableCollection<FuelSale>  RecentSales   { get; } = new ObservableCollection<FuelSale>();

        public RelayCommand RefreshCommand { get; }

        public DashboardViewModel()
        {
            RefreshCommand = new RelayCommand(_ => Load());
            Load();
        }

        public void Load()
        {
            try
            {
                TodayFuelRevenue = SaleService.GetTodayRevenue();
                TodayShopRevenue = ShopService.GetTodayRevenue();
                TodayFuelCount   = SaleService.GetTodayCount();
                ActiveEmployees  = EmployeeService.GetActiveCount();
                TotalCustomers   = CustomerService.GetTotalCount();

                OnPropertyChanged(nameof(TodayTotalRevenue));

                FuelLevels.Clear();
                foreach (var f in FuelService.GetAll())
                    FuelLevels.Add(f);

                RecentSales.Clear();
                var all = SaleService.GetAll();
                var take = Math.Min(all.Count, 10);
                for (int i = 0; i < take; i++)
                    RecentSales.Add(all[i]);
            }
            catch (Exception ex)
            {
                Logger.Error("Ошибка загрузки дашборда", ex);
            }
        }
    }
}
