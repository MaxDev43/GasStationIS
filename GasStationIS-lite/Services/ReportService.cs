using System;
using System.Collections.Generic;
using GasStationIS.Core;
using GasStationIS.Models;

namespace GasStationIS.Services
{
    /// <summary>
    /// Сервис формирования отчётов по топливу.
    /// </summary>
    public static class ReportService
    {
        // Продажи по видам топлива за период
        public static List<ReportEntry> GetFuelSalesByType(DateTime from, DateTime to) =>
            DatabaseService.ExecuteReader(
                "SELECT f.name AS label, " +
                "       SUM(s.liters) AS liters_sold, " +
                "       SUM(s.total_amount) AS revenue " +
                "FROM fuel_sales s " +
                "JOIN fuel_types f ON f.id = s.fuel_type_id " +
                "WHERE s.sale_datetime BETWEEN @from AND @to " +
                "GROUP BY f.name ORDER BY revenue DESC;",
                r => new ReportEntry
                {
                    Label = r["label"].ToString(),
                    Value = Convert.ToDouble(r["revenue"]),
                    Detail = $"{Convert.ToDouble(r["liters_sold"]):F0} л",
                },
                cmd =>
                {
                    cmd.Parameters.AddWithValue("@from", from.ToString("yyyy-MM-dd"));
                    cmd.Parameters.AddWithValue("@to", to.ToString("yyyy-MM-dd") + " 23:59:59");
                });

        // Текущие остатки в резервуарах
        public static List<ReportEntry> GetFuelStock() =>
            DatabaseService.ExecuteReader(
                "SELECT name AS label, stock_liters AS val, tank_capacity AS cap " +
                "FROM fuel_types ORDER BY name;",
                r => new ReportEntry
                {
                    Label = r["label"].ToString(),
                    Value = Convert.ToDouble(r["val"]),
                    Detail = $"Ёмкость: {Convert.ToDouble(r["cap"]):F0} л",
                });

        // Итоговые показатели за период
        public static (double FuelRevenue, long FuelSalesCount) GetSummary(DateTime from, DateTime to)
        {
            var fuelRev = DatabaseService.ExecuteScalar(
                "SELECT COALESCE(SUM(total_amount), 0) FROM fuel_sales " +
                "WHERE sale_datetime BETWEEN @from AND @to||' 23:59:59';",
                cmd =>
                {
                    cmd.Parameters.AddWithValue("@from", from.ToString("yyyy-MM-dd"));
                    cmd.Parameters.AddWithValue("@to", to.ToString("yyyy-MM-dd"));
                });

            var fuelCnt = DatabaseService.ExecuteScalar(
                "SELECT COUNT(*) FROM fuel_sales " +
                "WHERE sale_datetime BETWEEN @from AND @to||' 23:59:59';",
                cmd =>
                {
                    cmd.Parameters.AddWithValue("@from", from.ToString("yyyy-MM-dd"));
                    cmd.Parameters.AddWithValue("@to", to.ToString("yyyy-MM-dd"));
                });

            double fr = fuelRev == null || fuelRev == DBNull.Value ? 0 : Convert.ToDouble(fuelRev);
            return (fr, fuelCnt == null ? 0 : Convert.ToInt64(fuelCnt));
        }
    }
}