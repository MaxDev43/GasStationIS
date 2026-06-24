namespace GasStationIS.Models
{
    /// <summary>
    /// Сотрудник АЗС. Роль определяет доступные разделы интерфейса.
    /// Роль 'Администратор' — полный доступ.
    /// Роль 'Оператор' — только Главная, Топливо, Продажи, Клиенты.
    /// </summary>
    public class Employee
    {
        public int    Id           { get; set; }
        public string FullName     { get; set; }
        public string Role         { get; set; }        // Администратор / Оператор
        public string Login        { get; set; }
        public string PasswordHash { get; set; }
        public string Phone        { get; set; }
        public string HireDate     { get; set; }
        public bool   IsActive     { get; set; }

        public bool IsAdmin => Role == "Администратор";
    }
}
