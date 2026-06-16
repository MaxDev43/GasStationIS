using System;
using System.Collections.Generic;
using GasStationIS.Core;
using GasStationIS.Models;

namespace GasStationIS.Services
{
    public static class SaleService
    {
        public static List<FuelSale> GetAll() =>
            DatabaseService.ExecuteReader(
                "SELECT s.id, s.fuel_type_id, f.name AS fuel_name, s.liters, " +
                "       s.price_per_liter, s.total_amount, s.sale_datetime, " +
                "       s.payment_method, s.employee_id, e.full_name AS emp_name, " +
                "       s.customer_id, c.full_name AS cust_name, s.pump_number, s.notes " +
                "FROM fuel_sales s " +
                "JOIN fuel_types f ON f.id = s.fuel_type_id " +
                "LEFT JOIN employees e ON e.id = s.employee_id " +
                "LEFT JOIN customers c ON c.id = s.customer_id " +
                "ORDER BY s.sale_datetime DESC;",
                MapSale);

        public static List<FuelSale> GetByPeriod(DateTime from, DateTime to) =>
            DatabaseService.ExecuteReader(
                "SELECT s.id, s.fuel_type_id, f.name AS fuel_name, s.liters, " +
                "       s.price_per_liter, s.total_amount, s.sale_datetime, " +
                "       s.payment_method, s.employee_id, e.full_name AS emp_name, " +
                "       s.customer_id, c.full_name AS cust_name, s.pump_number, s.notes " +
                "FROM fuel_sales s " +
                "JOIN fuel_types f ON f.id = s.fuel_type_id " +
                "LEFT JOIN employees e ON e.id = s.employee_id " +
                "LEFT JOIN customers c ON c.id = s.customer_id " +
                "WHERE s.sale_datetime BETWEEN @from AND @to " +
                "ORDER BY s.sale_datetime DESC;",
                MapSale,
                cmd =>
                {
                    cmd.Parameters.AddWithValue("@from", from.ToString("yyyy-MM-dd"));
                    cmd.Parameters.AddWithValue("@to",   to.ToString("yyyy-MM-dd") + " 23:59:59");
                });

        public static double GetTodayRevenue()
        {
            var today  = DateTime.Now.ToString("yyyy-MM-dd");
            var result = DatabaseService.ExecuteScalar(
                "SELECT COALESCE(SUM(total_amount), 0) FROM fuel_sales " +
                "WHERE sale_datetime LIKE @today || '%';",
                cmd => cmd.Parameters.AddWithValue("@today", today));
            return result == null || result == DBNull.Value ? 0 : Convert.ToDouble(result);
        }

        public static long GetTodayCount()
        {
            var today  = DateTime.Now.ToString("yyyy-MM-dd");
            var result = DatabaseService.ExecuteScalar(
                "SELECT COUNT(*) FROM fuel_sales WHERE sale_datetime LIKE @today || '%';",
                cmd => cmd.Parameters.AddWithValue("@today", today));
            return result == null || result == DBNull.Value ? 0 : Convert.ToInt64(result);
        }

        /// <summary>
        /// Регистрирует продажу топлива и уменьшает остаток в резервуаре.
        /// Возвращает false если топлива недостаточно.
        /// </summary>
        public static bool Add(FuelSale sale)
        {
            if (sale == null) throw new ArgumentNullException(nameof(sale));
            if (sale.Liters <= 0) throw new ArgumentException("Объём должен быть > 0.");

            sale.TotalAmount = sale.Liters * sale.PricePerLiter;

            // Проверяем и списываем остаток
            if (!FuelService.DeductStock(sale.FuelTypeId, sale.Liters))
                return false;

            DatabaseService.ExecuteNonQuery(
                "INSERT INTO fuel_sales " +
                "(fuel_type_id, liters, price_per_liter, total_amount, sale_datetime, " +
                " payment_method, employee_id, customer_id, pump_number, notes) " +
                "VALUES (@fid, @lit, @ppl, @ta, @dt, @pm, @eid, @cid, @pump, @notes);",
                cmd =>
                {
                    cmd.Parameters.AddWithValue("@fid",   sale.FuelTypeId);
                    cmd.Parameters.AddWithValue("@lit",   sale.Liters);
                    cmd.Parameters.AddWithValue("@ppl",   sale.PricePerLiter);
                    cmd.Parameters.AddWithValue("@ta",    sale.TotalAmount);
                    cmd.Parameters.AddWithValue("@dt",    sale.SaleDatetime ?? DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                    cmd.Parameters.AddWithValue("@pm",    sale.PaymentMethod ?? "Наличные");
                    cmd.Parameters.AddWithValue("@eid",   (object)sale.EmployeeId ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@cid",   (object)sale.CustomerId ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@pump",  (object)sale.PumpNumber ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@notes", sale.Notes ?? "");
                });

            // Начисляем баллы клиенту (1 балл = 10 руб)
            if (sale.CustomerId.HasValue)
                CustomerService.AddLoyaltyPoints(sale.CustomerId.Value, sale.TotalAmount / 10.0, sale.TotalAmount);

            Logger.Info($"Продажа топлива: {sale.FuelTypeName}, {sale.Liters} л, {sale.TotalAmount:F2} руб.");
            return true;
        }

        public static void Delete(int id)
        {
            DatabaseService.ExecuteNonQuery(
                "DELETE FROM fuel_sales WHERE id = @id;",
                cmd => cmd.Parameters.AddWithValue("@id", id));
        }

        private static FuelSale MapSale(System.Data.IDataRecord r) => new FuelSale
        {
            Id            = Convert.ToInt32(r["id"]),
            FuelTypeId    = Convert.ToInt32(r["fuel_type_id"]),
            FuelTypeName  = r["fuel_name"].ToString(),
            Liters        = Convert.ToDouble(r["liters"]),
            PricePerLiter = Convert.ToDouble(r["price_per_liter"]),
            TotalAmount   = Convert.ToDouble(r["total_amount"]),
            SaleDatetime  = r["sale_datetime"].ToString(),
            PaymentMethod = r["payment_method"].ToString(),
            EmployeeId    = r["employee_id"] == DBNull.Value ? (int?)null : Convert.ToInt32(r["employee_id"]),
            EmployeeName  = r["emp_name"]?.ToString(),
            CustomerId    = r["customer_id"] == DBNull.Value ? (int?)null : Convert.ToInt32(r["customer_id"]),
            CustomerName  = r["cust_name"]?.ToString(),
            PumpNumber    = r["pump_number"] == DBNull.Value ? (int?)null : Convert.ToInt32(r["pump_number"]),
            Notes         = r["notes"]?.ToString(),
        };
    }
}
