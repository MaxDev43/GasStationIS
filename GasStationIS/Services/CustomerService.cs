using System;
using System.Collections.Generic;
using GasStationIS.Core;
using GasStationIS.Models;

namespace GasStationIS.Services
{
    public static class CustomerService
    {
        public static List<Customer> GetAll() =>
            DatabaseService.ExecuteReader(
                "SELECT id, full_name, phone, email, car_number, " +
                "       loyalty_points, total_spent, registration_date " +
                "FROM customers ORDER BY full_name;",
                MapCustomer);

        public static Customer GetById(int id)
        {
            var list = DatabaseService.ExecuteReader(
                "SELECT id, full_name, phone, email, car_number, " +
                "       loyalty_points, total_spent, registration_date " +
                "FROM customers WHERE id = @id;",
                MapCustomer,
                cmd => cmd.Parameters.AddWithValue("@id", id));
            return list.Count > 0 ? list[0] : null;
        }

        public static long GetTotalCount()
        {
            var r = DatabaseService.ExecuteScalar("SELECT COUNT(*) FROM customers;");
            return r == null ? 0 : Convert.ToInt64(r);
        }

        public static void Add(Customer c)
        {
            if (c == null) throw new ArgumentNullException(nameof(c));
            if (string.IsNullOrWhiteSpace(c.FullName)) throw new ArgumentException("Имя обязательно.");

            DatabaseService.ExecuteNonQuery(
                "INSERT INTO customers (full_name, phone, email, car_number, " +
                "                       loyalty_points, total_spent, registration_date) " +
                "VALUES (@fn, @ph, @em, @cn, 0, 0, @rd);",
                cmd =>
                {
                    cmd.Parameters.AddWithValue("@fn", c.FullName);
                    cmd.Parameters.AddWithValue("@ph", c.Phone     ?? "");
                    cmd.Parameters.AddWithValue("@em", c.Email     ?? "");
                    cmd.Parameters.AddWithValue("@cn", c.CarNumber ?? "");
                    cmd.Parameters.AddWithValue("@rd", DateTime.Now.ToString("yyyy-MM-dd"));
                });

            Logger.Info($"Добавлен клиент: {c.FullName}");
        }

        public static void Update(Customer c)
        {
            if (c == null) throw new ArgumentNullException(nameof(c));
            DatabaseService.ExecuteNonQuery(
                "UPDATE customers SET full_name=@fn, phone=@ph, email=@em, car_number=@cn WHERE id=@id;",
                cmd =>
                {
                    cmd.Parameters.AddWithValue("@fn", c.FullName);
                    cmd.Parameters.AddWithValue("@ph", c.Phone     ?? "");
                    cmd.Parameters.AddWithValue("@em", c.Email     ?? "");
                    cmd.Parameters.AddWithValue("@cn", c.CarNumber ?? "");
                    cmd.Parameters.AddWithValue("@id", c.Id);
                });
        }

        public static void Delete(int id) =>
            DatabaseService.ExecuteNonQuery(
                "DELETE FROM customers WHERE id=@id;",
                cmd => cmd.Parameters.AddWithValue("@id", id));

        public static void AddLoyaltyPoints(int customerId, double points, double spent)
        {
            DatabaseService.ExecuteNonQuery(
                "UPDATE customers SET loyalty_points = loyalty_points + @pts, " +
                "                     total_spent    = total_spent    + @spent " +
                "WHERE id = @id;",
                cmd =>
                {
                    cmd.Parameters.AddWithValue("@pts",   points);
                    cmd.Parameters.AddWithValue("@spent", spent);
                    cmd.Parameters.AddWithValue("@id",    customerId);
                });
        }

        private static Customer MapCustomer(System.Data.IDataRecord r) => new Customer
        {
            Id               = Convert.ToInt32(r["id"]),
            FullName         = r["full_name"].ToString(),
            Phone            = r["phone"]?.ToString(),
            Email            = r["email"]?.ToString(),
            CarNumber        = r["car_number"]?.ToString(),
            LoyaltyPoints    = Convert.ToDouble(r["loyalty_points"]),
            TotalSpent       = Convert.ToDouble(r["total_spent"]),
            RegistrationDate = r["registration_date"].ToString(),
        };
    }
}
