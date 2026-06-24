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
                "SELECT id, full_name, phone, car_number, registration_date " +
                "FROM customers ORDER BY full_name;",
                MapCustomer);

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
                "INSERT INTO customers (full_name, phone, car_number, registration_date) " +
                "VALUES (@fn, @ph, @cn, @rd);",
                cmd =>
                {
                    cmd.Parameters.AddWithValue("@fn", c.FullName);
                    cmd.Parameters.AddWithValue("@ph", c.Phone ?? "");
                    cmd.Parameters.AddWithValue("@cn", c.CarNumber ?? "");
                    cmd.Parameters.AddWithValue("@rd", DateTime.Now.ToString("yyyy-MM-dd"));
                });

            Logger.Info($"Добавлен клиент: {c.FullName}");
        }

        public static void Update(Customer c)
        {
            if (c == null) throw new ArgumentNullException(nameof(c));
            DatabaseService.ExecuteNonQuery(
                "UPDATE customers SET full_name=@fn, phone=@ph, car_number=@cn WHERE id=@id;",
                cmd =>
                {
                    cmd.Parameters.AddWithValue("@fn", c.FullName);
                    cmd.Parameters.AddWithValue("@ph", c.Phone ?? "");
                    cmd.Parameters.AddWithValue("@cn", c.CarNumber ?? "");
                    cmd.Parameters.AddWithValue("@id", c.Id);
                });
        }

        public static void Delete(int id) =>
            DatabaseService.ExecuteNonQuery(
                "DELETE FROM customers WHERE id=@id;",
                cmd => cmd.Parameters.AddWithValue("@id", id));

        private static Customer MapCustomer(System.Data.IDataRecord r) => new Customer
        {
            Id = Convert.ToInt32(r["id"]),
            FullName = r["full_name"].ToString(),
            Phone = r["phone"]?.ToString(),
            CarNumber = r["car_number"]?.ToString(),
            RegistrationDate = r["registration_date"].ToString(),
        };
    }
}