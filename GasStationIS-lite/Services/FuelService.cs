using System;
using System.Collections.Generic;
using GasStationIS.Core;
using GasStationIS.Models;

namespace GasStationIS.Services
{
    public static class FuelService
    {
        // ── Чтение ──────────────────────────────────────────────────────────
        public static List<FuelType> GetAll() =>
            DatabaseService.ExecuteReader(
                "SELECT id, name, price_per_liter, stock_liters, tank_capacity, last_updated " +
                "FROM fuel_types ORDER BY id;",
                MapFuelType);

        public static FuelType GetById(int id)
        {
            var list = DatabaseService.ExecuteReader(
                "SELECT id, name, price_per_liter, stock_liters, tank_capacity, last_updated " +
                "FROM fuel_types WHERE id = @id;",
                MapFuelType,
                cmd => cmd.Parameters.AddWithValue("@id", id));
            return list.Count > 0 ? list[0] : null;
        }

        // ── Обновление цены ──────────────────────────────────────────────────
        public static void UpdatePrice(int id, double newPrice)
        {
            if (newPrice < 0) throw new ArgumentException("Цена не может быть отрицательной.");

            DatabaseService.ExecuteNonQuery(
                "UPDATE fuel_types SET price_per_liter = @price, last_updated = @date WHERE id = @id;",
                cmd =>
                {
                    cmd.Parameters.AddWithValue("@price", newPrice);
                    cmd.Parameters.AddWithValue("@date",  DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                    cmd.Parameters.AddWithValue("@id",    id);
                });

            Logger.Info($"Обновлена цена топлива id={id} → {newPrice:F2} руб/л");
        }

        // ── Пополнение остатка ───────────────────────────────────────────────
        public static void AddStock(int fuelTypeId, double liters)
        {
            if (liters <= 0) throw new ArgumentException("Объём должен быть положительным.");

            DatabaseService.ExecuteNonQuery(
                "UPDATE fuel_types " +
                "SET stock_liters = MIN(stock_liters + @liters, tank_capacity), " +
                "    last_updated = @date " +
                "WHERE id = @id;",
                cmd =>
                {
                    cmd.Parameters.AddWithValue("@liters", liters);
                    cmd.Parameters.AddWithValue("@date",   DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                    cmd.Parameters.AddWithValue("@id",     fuelTypeId);
                });
        }

        // ── Списание при продаже ─────────────────────────────────────────────
        public static bool DeductStock(int fuelTypeId, double liters)
        {
            var fuel = GetById(fuelTypeId);
            if (fuel == null || fuel.StockLiters < liters)
                return false;

            DatabaseService.ExecuteNonQuery(
                "UPDATE fuel_types " +
                "SET stock_liters = stock_liters - @liters, last_updated = @date " +
                "WHERE id = @id;",
                cmd =>
                {
                    cmd.Parameters.AddWithValue("@liters", liters);
                    cmd.Parameters.AddWithValue("@date",   DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                    cmd.Parameters.AddWithValue("@id",     fuelTypeId);
                });

            return true;
        }

        // ── Маппер ───────────────────────────────────────────────────────────
        private static FuelType MapFuelType(System.Data.IDataRecord r) => new FuelType
        {
            Id           = Convert.ToInt32(r["id"]),
            Name         = r["name"].ToString(),
            PricePerLiter = Convert.ToDouble(r["price_per_liter"]),
            StockLiters  = Convert.ToDouble(r["stock_liters"]),
            TankCapacity  = Convert.ToDouble(r["tank_capacity"]),
            LastUpdated  = r["last_updated"].ToString(),
        };
    }
}
