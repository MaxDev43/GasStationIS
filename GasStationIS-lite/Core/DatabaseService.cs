using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.IO;

namespace GasStationIS.Core
{
    public static class DatabaseService
    {
        // Путь к файлу БД — лежит рядом с .exe
        private static readonly string DbDirectory = AppDomain.CurrentDomain.BaseDirectory;
        private static readonly string DbFileName = "azs_network.db";
        public static readonly string DbPath = Path.Combine(DbDirectory, DbFileName);

        public static string ConnectionString =>
            $"Data Source={DbPath};Version=3;Foreign Keys=True;";

        // Возвращает открытое соединение. Закрывать через using.
        public static SQLiteConnection GetOpenConnection()
        {
            var conn = new SQLiteConnection(ConnectionString);
            conn.Open();
            using (var cmd = new SQLiteCommand("PRAGMA foreign_keys = ON;", conn))
                cmd.ExecuteNonQuery();
            return conn;
        }

        // INSERT / UPDATE / DELETE без возвращаемого значения
        public static void ExecuteNonQuery(string sql, Action<SQLiteCommand> addParams = null)
        {
            using (var conn = GetOpenConnection())
            using (var cmd = new SQLiteCommand(sql, conn))
            {
                addParams?.Invoke(cmd);
                cmd.ExecuteNonQuery();
            }
        }

        // SELECT одного скалярного значения
        public static object ExecuteScalar(string sql, Action<SQLiteCommand> addParams = null)
        {
            using (var conn = GetOpenConnection())
            using (var cmd = new SQLiteCommand(sql, conn))
            {
                addParams?.Invoke(cmd);
                return cmd.ExecuteScalar();
            }
        }

        // SELECT с построчным чтением через callback
        public static List<T> ExecuteReader<T>(
            string sql,
            Func<IDataRecord, T> map,
            Action<SQLiteCommand> addParams = null)
        {
            var result = new List<T>();
            using (var conn = GetOpenConnection())
            using (var cmd = new SQLiteCommand(sql, conn))
            {
                addParams?.Invoke(cmd);
                using (var reader = cmd.ExecuteReader())
                    while (reader.Read())
                        result.Add(map(reader));
            }
            return result;
        }

        // Вызывается один раз при старте из App.xaml.cs
        public static void InitializeDatabase()
        {
            Logger.Info("Инициализация базы данных...");

            using (var conn = GetOpenConnection())
            {
                ExecuteSchema(conn, CreateStationInfoTable);
                ExecuteSchema(conn, CreateEmployeesTable);
                ExecuteSchema(conn, CreateFuelTypesTable);
                ExecuteSchema(conn, CreateFuelSuppliesTable);
                ExecuteSchema(conn, CreateFuelSalesTable);
                ExecuteSchema(conn, CreateCustomersTable);

                SeedDefaultData(conn);
            }

            Logger.Info("База данных инициализирована успешно.");
        }

        private static void ExecuteSchema(SQLiteConnection conn, string sql)
        {
            using (var cmd = new SQLiteCommand(sql, conn))
                cmd.ExecuteNonQuery();
        }

        // ── DDL ───────────────────────────────────────────────────────────

        private const string CreateStationInfoTable = @"
            CREATE TABLE IF NOT EXISTS station_info (
                id              INTEGER PRIMARY KEY,
                name            TEXT    NOT NULL DEFAULT 'АЗС №1',
                address         TEXT,
                phone           TEXT,
                director        TEXT,
                inn             TEXT,
                license_number  TEXT
            );";

        private const string CreateEmployeesTable = @"
            CREATE TABLE IF NOT EXISTS employees (
                id              INTEGER PRIMARY KEY AUTOINCREMENT,
                full_name       TEXT    NOT NULL,
                role            TEXT    NOT NULL CHECK(role IN ('Администратор','Оператор')),
                login           TEXT    UNIQUE NOT NULL,
                password_hash   TEXT    NOT NULL,
                phone           TEXT,
                hire_date       TEXT    NOT NULL,
                is_active       INTEGER NOT NULL DEFAULT 1
            );";

        private const string CreateFuelTypesTable = @"
            CREATE TABLE IF NOT EXISTS fuel_types (
                id              INTEGER PRIMARY KEY AUTOINCREMENT,
                name            TEXT    NOT NULL UNIQUE,
                price_per_liter REAL    NOT NULL DEFAULT 0,
                stock_liters    REAL    NOT NULL DEFAULT 0,
                tank_capacity   REAL    NOT NULL DEFAULT 50000,
                last_updated    TEXT    NOT NULL
            );";

        // Поставки топлива: кто привёз, сколько и по какой цене
        private const string CreateFuelSuppliesTable = @"
            CREATE TABLE IF NOT EXISTS fuel_supplies (
                id              INTEGER PRIMARY KEY AUTOINCREMENT,
                supplier_name   TEXT    NOT NULL,
                fuel_type_id    INTEGER NOT NULL REFERENCES fuel_types(id),
                quantity_liters REAL    NOT NULL,
                price_per_liter REAL    NOT NULL,
                total_cost      REAL    NOT NULL,
                supply_date     TEXT    NOT NULL,
                employee_id     INTEGER REFERENCES employees(id)
            );";

        // Продажи топлива: каждый отпуск на колонке
        private const string CreateFuelSalesTable = @"
            CREATE TABLE IF NOT EXISTS fuel_sales (
                id              INTEGER PRIMARY KEY AUTOINCREMENT,
                fuel_type_id    INTEGER NOT NULL REFERENCES fuel_types(id),
                liters          REAL    NOT NULL,
                price_per_liter REAL    NOT NULL,
                total_amount    REAL    NOT NULL,
                sale_datetime   TEXT    NOT NULL,
                payment_method  TEXT    NOT NULL DEFAULT 'Наличные',
                employee_id     INTEGER REFERENCES employees(id),
                customer_id     INTEGER REFERENCES customers(id)
            );";

        // Клиенты АЗС
        private const string CreateCustomersTable = @"
            CREATE TABLE IF NOT EXISTS customers (
                id                INTEGER PRIMARY KEY AUTOINCREMENT,
                full_name         TEXT    NOT NULL,
                phone             TEXT,
                car_number        TEXT,
                registration_date TEXT    NOT NULL
            );";

        // ── Начальные данные ─────────────────────────────────────────────

        private static void SeedDefaultData(SQLiteConnection conn)
        {
            // Информация об АЗС
            using (var cmd = new SQLiteCommand(
                "INSERT OR IGNORE INTO station_info (id, name, address, director) " +
                "VALUES (1, 'АЗС №1 — Главная', 'г. Москва, ул. Примерная, д. 1', 'Иванов И.И.');", conn))
                cmd.ExecuteNonQuery();

            // Администратор по умолчанию (пароль: admin123)
            using (var cmd = new SQLiteCommand(
                "INSERT OR IGNORE INTO employees " +
                "(full_name, role, login, password_hash, phone, hire_date, is_active) " +
                "VALUES ('Администратор системы', 'Администратор', 'admin', " +
                "'240be518fabd2724ddb6f04eeb1da5967448d7e831c08c8fa822809f74c720a9', " +
                "'+7 (999) 000-00-00', @date, 1);", conn))
            {
                cmd.Parameters.AddWithValue("@date", DateTime.Now.ToString("yyyy-MM-dd"));
                cmd.ExecuteNonQuery();
            }

            // Виды топлива
            var fuels = new[]
            {
                ("АИ-92", 52.50, 35000.0),
                ("АИ-95", 56.80, 40000.0),
                ("АИ-98", 63.20, 20000.0),
                ("ДТ",    58.90, 45000.0),
                ("Газ",   34.50, 10000.0),
            };

            foreach (var (name, price, stock) in fuels)
            {
                using (var cmd = new SQLiteCommand(
                    "INSERT OR IGNORE INTO fuel_types " +
                    "(name, price_per_liter, stock_liters, tank_capacity, last_updated) " +
                    "VALUES (@name, @price, @stock, 50000, @date);", conn))
                {
                    cmd.Parameters.AddWithValue("@name", name);
                    cmd.Parameters.AddWithValue("@price", price);
                    cmd.Parameters.AddWithValue("@stock", stock);
                    cmd.Parameters.AddWithValue("@date", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                    cmd.ExecuteNonQuery();
                }
            }
        }
    }
}