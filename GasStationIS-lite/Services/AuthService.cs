using System;
using System.Security.Cryptography;
using System.Text;
using GasStationIS.Core;
using GasStationIS.Models;

namespace GasStationIS.Services
{
    /// <summary>
    /// Сервис аутентификации.
    /// Пароли хранятся в виде SHA-256 хэша.
    /// </summary>
    public static class AuthService
    {
        /// <summary>
        /// Вычисляет SHA-256 хэш пароля.
        /// Пароль по умолчанию для admin: admin123
        /// Хэш:  240be518fabd2724ddb6f04eeb1da5967448d7e831c08c8fa822809f74c720a9
        /// </summary>
        public static string HashPassword(string password)
        {
            if (string.IsNullOrEmpty(password))
                throw new ArgumentException("Пароль не может быть пустым.", nameof(password));

            using (var sha256 = SHA256.Create())
            {
                var bytes = Encoding.UTF8.GetBytes(password);
                var hash  = sha256.ComputeHash(bytes);
                return BitConverter.ToString(hash)
                                   .Replace("-", "")
                                   .ToLowerInvariant();
            }
        }

        /// <summary>
        /// Проверяет логин/пароль и возвращает сотрудника или null.
        /// </summary>
        public static Employee Login(string login, string password)
        {
            if (string.IsNullOrWhiteSpace(login) || string.IsNullOrWhiteSpace(password))
                return null;

            var hash = HashPassword(password);

            var employees = DatabaseService.ExecuteReader(
                "SELECT id, full_name, role, login, password_hash, phone, hire_date, is_active " +
                "FROM employees " +
                "WHERE login = @login AND password_hash = @hash AND is_active = 1;",
                r => new Employee
                {
                    Id           = Convert.ToInt32(r["id"]),
                    FullName     = r["full_name"].ToString(),
                    Role         = r["role"].ToString(),
                    Login        = r["login"].ToString(),
                    PasswordHash = r["password_hash"].ToString(),
                    Phone        = r["phone"]?.ToString(),
                    HireDate     = r["hire_date"].ToString(),
                    IsActive     = Convert.ToInt32(r["is_active"]) == 1,
                },
                cmd =>
                {
                    cmd.Parameters.AddWithValue("@login", login.Trim());
                    cmd.Parameters.AddWithValue("@hash",  hash);
                });

            return employees.Count > 0 ? employees[0] : null;
        }
    }
}
