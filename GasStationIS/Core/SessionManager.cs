using GasStationIS.Models;

namespace GasStationIS.Core
{
    /// <summary>
    /// Хранит данные текущей авторизованной сессии.
    /// Singleton — живёт всё время работы приложения.
    /// </summary>
    public static class SessionManager
    {
        /// <summary>Текущий авторизованный сотрудник.</summary>
        public static Employee CurrentEmployee { get; private set; }

        /// <summary>Является ли текущий пользователь администратором.</summary>
        public static bool IsAdmin =>
            CurrentEmployee?.Role == "Администратор";

        /// <summary>Является ли кто-то авторизованным.</summary>
        public static bool IsAuthenticated =>
            CurrentEmployee != null;

        public static void Login(Employee employee)
        {
            CurrentEmployee = employee;
            Logger.Info($"Вход: {employee.FullName} ({employee.Role})");
        }

        public static void Logout()
        {
            Logger.Info($"Выход: {CurrentEmployee?.FullName}");
            CurrentEmployee = null;
        }
    }
}
