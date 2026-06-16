using System;
using System.Collections.Generic;
using GasStationIS.Core;
using GasStationIS.Models;

namespace GasStationIS.Services
{
    public static class SupplyService
    {
        public static List<FuelSupply> GetAll() =>
            DatabaseService.ExecuteReader(
                "SELECT s.id, s.supplier_name, s.supplier_phone, s.fuel_type_id, " +
                "       f.name AS fuel_name, s.quantity_liters, s.price_per_liter, " +
                "       s.total_cost, s.supply_date, s.employee_id, " +
                "       e.full_name AS emp_name, s.notes " +
                "FROM fuel_supplies s " +
                "JOIN fuel_types f ON f.id = s.fuel_type_id " +
                "LEFT JOIN employees e ON e.id = s.employee_id " +
                "ORDER BY s.supply_date DESC;",
                MapSupply);

        public static List<FuelSupply> GetByPeriod(DateTime from, DateTime to) =>
            DatabaseService.ExecuteReader(
                "SELECT s.id, s.supplier_name, s.supplier_phone, s.fuel_type_id, " +
                "       f.name AS fuel_name, s.quantity_liters, s.price_per_liter, " +
                "       s.total_cost, s.supply_date, s.employee_id, " +
                "       e.full_name AS emp_name, s.notes " +
                "FROM fuel_supplies s " +
                "JOIN fuel_types f ON f.id = s.fuel_type_id " +
                "LEFT JOIN employees e ON e.id = s.employee_id " +
                "WHERE s.supply_date BETWEEN @from AND @to " +
                "ORDER BY s.supply_date DESC;",
                MapSupply,
                cmd =>
                {
                    cmd.Parameters.AddWithValue("@from", from.ToString("yyyy-MM-dd"));
                    cmd.Parameters.AddWithValue("@to",   to.ToString("yyyy-MM-dd") + " 23:59:59");
                });

        public static void Add(FuelSupply supply)
        {
            if (supply == null) throw new ArgumentNullException(nameof(supply));
            if (supply.QuantityLiters <= 0) throw new ArgumentException("Объём должен быть > 0.");
            if (supply.PricePerLiter  <= 0) throw new ArgumentException("Цена должна быть > 0.");

            supply.TotalCost = supply.QuantityLiters * supply.PricePerLiter;

            DatabaseService.ExecuteNonQuery(
                "INSERT INTO fuel_supplies " +
                "(supplier_name, supplier_phone, fuel_type_id, quantity_liters, " +
                " price_per_liter, total_cost, supply_date, employee_id, notes) " +
                "VALUES (@sn, @sp, @fid, @qty, @ppl, @tc, @date, @eid, @notes);",
                cmd =>
                {
                    cmd.Parameters.AddWithValue("@sn",    supply.SupplierName);
                    cmd.Parameters.AddWithValue("@sp",    supply.SupplierPhone ?? "");
                    cmd.Parameters.AddWithValue("@fid",   supply.FuelTypeId);
                    cmd.Parameters.AddWithValue("@qty",   supply.QuantityLiters);
                    cmd.Parameters.AddWithValue("@ppl",   supply.PricePerLiter);
                    cmd.Parameters.AddWithValue("@tc",    supply.TotalCost);
                    cmd.Parameters.AddWithValue("@date",  supply.SupplyDate ?? DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                    cmd.Parameters.AddWithValue("@eid",   (object)supply.EmployeeId ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@notes", supply.Notes ?? "");
                });

            // Пополняем остаток топлива
            FuelService.AddStock(supply.FuelTypeId, supply.QuantityLiters);

            Logger.Info($"Поставка: {supply.SupplierName}, {supply.FuelTypeName}, {supply.QuantityLiters} л");
        }

        public static void Delete(int id)
        {
            DatabaseService.ExecuteNonQuery(
                "DELETE FROM fuel_supplies WHERE id = @id;",
                cmd => cmd.Parameters.AddWithValue("@id", id));
        }

        private static FuelSupply MapSupply(System.Data.IDataRecord r) => new FuelSupply
        {
            Id             = Convert.ToInt32(r["id"]),
            SupplierName   = r["supplier_name"].ToString(),
            SupplierPhone  = r["supplier_phone"]?.ToString(),
            FuelTypeId     = Convert.ToInt32(r["fuel_type_id"]),
            FuelTypeName   = r["fuel_name"].ToString(),
            QuantityLiters = Convert.ToDouble(r["quantity_liters"]),
            PricePerLiter  = Convert.ToDouble(r["price_per_liter"]),
            TotalCost      = Convert.ToDouble(r["total_cost"]),
            SupplyDate     = r["supply_date"].ToString(),
            EmployeeId     = r["employee_id"] == DBNull.Value ? (int?)null : Convert.ToInt32(r["employee_id"]),
            EmployeeName   = r["emp_name"]?.ToString(),
            Notes          = r["notes"]?.ToString(),
        };
    }
}
