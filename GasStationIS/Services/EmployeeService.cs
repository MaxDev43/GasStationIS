using System;
using System.Collections.Generic;
using GasStationIS.Core;
using GasStationIS.Models;

namespace GasStationIS.Services
{
    public static class EmployeeService
    {
        public static List<Employee> GetAll() =>
            DatabaseService.ExecuteReader(
                "SELECT id, full_name, role, login, password_hash, phone, hire_date, is_active " +
                "FROM employees ORDER BY full_name;",
                MapEmployee);

        public static List<Employee> GetActive() =>
            DatabaseService.ExecuteReader(
                "SELECT id, full_name, role, login, password_hash, phone, hire_date, is_active " +
                "FROM employees WHERE is_active = 1 ORDER BY full_name;",
                MapEmployee);

        public static long GetActiveCount()
        {
            var r = DatabaseService.ExecuteScalar("SELECT COUNT(*) FROM employees WHERE is_active=1;");
            return r == null ? 0 : Convert.ToInt64(r);
        }

        public static void Add(Employee emp, string plainPassword)
        {
            if (emp == null) throw new ArgumentNullException(nameof(emp));
            if (string.IsNullOrWhiteSpace(emp.FullName)) throw new ArgumentException("Имя обязательно.");
            if (string.IsNullOrWhiteSpace(plainPassword)) throw new ArgumentException("Пароль обязателен.");

            var hash = AuthService.HashPassword(plainPassword);

            DatabaseService.ExecuteNonQuery(
                "INSERT INTO employees " +
                "(full_name, role, login, password_hash, phone, hire_date, is_active) " +
                "VALUES (@fn, @role, @login, @hash, @phone, @hd, 1);",
                cmd =>
                {
                    cmd.Parameters.AddWithValue("@fn",    emp.FullName);
                    cmd.Parameters.AddWithValue("@role",  emp.Role);
                    cmd.Parameters.AddWithValue("@login", emp.Login);
                    cmd.Parameters.AddWithValue("@hash",  hash);
                    cmd.Parameters.AddWithValue("@phone", emp.Phone ?? "");
                    cmd.Parameters.AddWithValue("@hd",    emp.HireDate ?? DateTime.Now.ToString("yyyy-MM-dd"));
                });

            Logger.Info($"Добавлен сотрудник: {emp.FullName} ({emp.Role})");
        }

        public static void Update(Employee emp)
        {
            if (emp == null) throw new ArgumentNullException(nameof(emp));
            DatabaseService.ExecuteNonQuery(
                "UPDATE employees SET full_name=@fn, role=@role, login=@login, phone=@phone WHERE id=@id;",
                cmd =>
                {
                    cmd.Parameters.AddWithValue("@fn",    emp.FullName);
                    cmd.Parameters.AddWithValue("@role",  emp.Role);
                    cmd.Parameters.AddWithValue("@login", emp.Login);
                    cmd.Parameters.AddWithValue("@phone", emp.Phone ?? "");
                    cmd.Parameters.AddWithValue("@id",    emp.Id);
                });
        }

        public static void ChangePassword(int id, string newPlainPassword)
        {
            if (string.IsNullOrWhiteSpace(newPlainPassword))
                throw new ArgumentException("Новый пароль не может быть пустым.");

            var hash = AuthService.HashPassword(newPlainPassword);
            DatabaseService.ExecuteNonQuery(
                "UPDATE employees SET password_hash=@hash WHERE id=@id;",
                cmd =>
                {
                    cmd.Parameters.AddWithValue("@hash", hash);
                    cmd.Parameters.AddWithValue("@id",   id);
                });
        }

        public static void SetActive(int id, bool isActive) =>
            DatabaseService.ExecuteNonQuery(
                "UPDATE employees SET is_active=@active WHERE id=@id;",
                cmd =>
                {
                    cmd.Parameters.AddWithValue("@active", isActive ? 1 : 0);
                    cmd.Parameters.AddWithValue("@id",     id);
                });

        private static Employee MapEmployee(System.Data.IDataRecord r) => new Employee
        {
            Id           = Convert.ToInt32(r["id"]),
            FullName     = r["full_name"].ToString(),
            Role         = r["role"].ToString(),
            Login        = r["login"].ToString(),
            PasswordHash = r["password_hash"].ToString(),
            Phone        = r["phone"]?.ToString(),
            HireDate     = r["hire_date"].ToString(),
            IsActive     = Convert.ToInt32(r["is_active"]) == 1,
        };
    }
}
