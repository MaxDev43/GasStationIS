using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.IO;

namespace GasStationIS.Core
{
    public static class DatabaseService
    {
        // ── Путь к файлу БД ──────────────────────────────────────────────────
        private static readonly string DbDirectory =
            AppDomain.CurrentDomain.BaseDirectory;

        private static readonly string DbFileName = "azs_network.db";

        public static readonly string DbPath =
            Path.Combine(DbDirectory, DbFileName);

        public static string ConnectionString =>
            $"Data Source={DbPath};Version=3;Foreign Keys=True;";

        // ── Фабрика соединений ───────────────────────────────────────────────
        /// Возвращает открытое соединение. Вызывающий код обязан закрыть его через using.
        public static SQLiteConnection GetOpenConnection()
        {
            var conn = new SQLiteConnection(ConnectionString);
            conn.Open();
            // Включаем поддержку внешних ключей для каждого соединения
            using (var cmd = new SQLiteCommand("PRAGMA foreign_keys = ON;", conn))
                cmd.ExecuteNonQuery();
            return conn;
        }

        // ── Вспомогательные методы ───────────────────────────────────────────
        /// <summary>INSERT/UPDATE/DELETE без возвращаемого значения.</summary>
        public static void ExecuteNonQuery(string sql, Action<SQLiteCommand> addParams = null)
        {
            using (var conn = GetOpenConnection())
            using (var cmd  = new SQLiteCommand(sql, conn))
            {
                addParams?.Invoke(cmd);
                cmd.ExecuteNonQuery();
            }
        }

        /// <summary>SELECT одного скалярного значения.</summary>
        public static object ExecuteScalar(string sql, Action<SQLiteCommand> addParams = null)
        {
            using (var conn = GetOpenConnection())
            using (var cmd  = new SQLiteCommand(sql, conn))
            {
                addParams?.Invoke(cmd);
                return cmd.ExecuteScalar();
            }
        }

        /// <summary>SELECT в DataTable.</summary>
        public static DataTable ExecuteQuery(string sql, Action<SQLiteCommand> addParams = null)
        {
            var dt = new DataTable();
            using (var conn = GetOpenConnection())
            using (var cmd  = new SQLiteCommand(sql, conn))
            {
                addParams?.Invoke(cmd);
                using (var adapter = new SQLiteDataAdapter(cmd))
                    adapter.Fill(dt);
            }
            return dt;
        }

        /// <summary>SELECT с построчным чтением через callback.</summary>
        public static List<T> ExecuteReader<T>(
            string sql,
            Func<IDataRecord, T> map,
            Action<SQLiteCommand> addParams = null)
        {
            var result = new List<T>();
            using (var conn = GetOpenConnection())
            using (var cmd  = new SQLiteCommand(sql, conn))
            {
                addParams?.Invoke(cmd);
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                        result.Add(map(reader));
                }
            }
            return result;
        }

        // ── Инициализация схемы БД ───────────────────────────────────────────
        /// Вызывается из App.xaml.cs один раз при старте.
        public static void InitializeDatabase()
        {
            Logger.Info("Инициализация базы данных...");

            // SQLite создаёт файл автоматически при первом подключении
            using (var conn = GetOpenConnection())
            {
                ExecuteSchema(conn, CreateStationInfoTable);
                ExecuteSchema(conn, CreateEmployeesTable);
                ExecuteSchema(conn, CreateFuelTypesTable);
                ExecuteSchema(conn, CreateFuelSuppliesTable);
                ExecuteSchema(conn, CreateFuelSalesTable);
                ExecuteSchema(conn, CreateShopProductsTable);
                ExecuteSchema(conn, CreateShopSalesTable);
                ExecuteSchema(conn, CreateShopSaleItemsTable);
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

        // ── DDL ───────────────────────────────────────────────────────────────

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

        private const string CreateFuelSuppliesTable = @"
            CREATE TABLE IF NOT EXISTS fuel_supplies (
                id              INTEGER PRIMARY KEY AUTOINCREMENT,
                supplier_name   TEXT    NOT NULL,
                supplier_phone  TEXT,
                fuel_type_id    INTEGER NOT NULL REFERENCES fuel_types(id),
                quantity_liters REAL    NOT NULL,
                price_per_liter REAL    NOT NULL,
                total_cost      REAL    NOT NULL,
                supply_date     TEXT    NOT NULL,
                employee_id     INTEGER REFERENCES employees(id),
                notes           TEXT
            );";

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
                customer_id     INTEGER REFERENCES customers(id),
                pump_number     INTEGER,
                notes           TEXT
            );";

        private const string CreateShopProductsTable = @"
            CREATE TABLE IF NOT EXISTS shop_products (
                id              INTEGER PRIMARY KEY AUTOINCREMENT,
                name            TEXT    NOT NULL,
                category        TEXT,
                price           REAL    NOT NULL DEFAULT 0,
                stock           INTEGER NOT NULL DEFAULT 0,
                barcode         TEXT,
                last_updated    TEXT    NOT NULL
            );";

        private const string CreateShopSalesTable = @"
            CREATE TABLE IF NOT EXISTS shop_sales (
                id              INTEGER PRIMARY KEY AUTOINCREMENT,
                sale_datetime   TEXT    NOT NULL,
                total_amount    REAL    NOT NULL,
                payment_method  TEXT    NOT NULL DEFAULT 'Наличные',
                employee_id     INTEGER REFERENCES employees(id)
            );";

        private const string CreateShopSaleItemsTable = @"
            CREATE TABLE IF NOT EXISTS shop_sale_items (
                id              INTEGER PRIMARY KEY AUTOINCREMENT,
                sale_id         INTEGER NOT NULL REFERENCES shop_sales(id),
                product_id      INTEGER NOT NULL REFERENCES shop_products(id),
                product_name    TEXT    NOT NULL,
                quantity        INTEGER NOT NULL,
                price           REAL    NOT NULL
            );";

        private const string CreateCustomersTable = @"
            CREATE TABLE IF NOT EXISTS customers (
                id                  INTEGER PRIMARY KEY AUTOINCREMENT,
                full_name           TEXT    NOT NULL,
                phone               TEXT,
                email               TEXT,
                car_number          TEXT,
                loyalty_points      REAL    NOT NULL DEFAULT 0,
                total_spent         REAL    NOT NULL DEFAULT 0,
                registration_date   TEXT    NOT NULL
            );";

        // ── Начальные данные ─────────────────────────────────────────────────
        private static void SeedDefaultData(SQLiteConnection conn)
        {
            // Информация об АЗС
            using (var cmd = new SQLiteCommand(
                "INSERT OR IGNORE INTO station_info (id, name, address, director) " +
                "VALUES (1, 'АЗС №1 — Главная', 'г. Москва, ул. Примерная, д. 1', 'Иванов И.И.');", conn))
                cmd.ExecuteNonQuery();

            // Администратор по умолчанию (пароль: admin123 → SHA256)
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
                ("АИ-92", 52.50m, 35000.0),
                ("АИ-95", 56.80m, 40000.0),
                ("АИ-98", 63.20m, 20000.0),
                ("ДТ",    58.90m, 45000.0),
                ("Газ",   34.50m, 10000.0),
            };

            foreach (var (name, price, stock) in fuels)
            {
                using (var cmd = new SQLiteCommand(
                    "INSERT OR IGNORE INTO fuel_types " +
                    "(name, price_per_liter, stock_liters, tank_capacity, last_updated) " +
                    "VALUES (@name, @price, @stock, 50000, @date);", conn))
                {
                    cmd.Parameters.AddWithValue("@name",  name);
                    cmd.Parameters.AddWithValue("@price", (double)price);
                    cmd.Parameters.AddWithValue("@stock", stock);
                    cmd.Parameters.AddWithValue("@date",  DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                    cmd.ExecuteNonQuery();
                }
            }

            // Тестовые товары магазина
            var products = new[]
            {
                ("Моторное масло 5W-30 (1л)",  "Масла и жидкости", 680.0, 50),
                ("Антифриз G12 (1л)",          "Масла и жидкости", 290.0, 80),
                ("Жидкость стеклоомывателя",   "Масла и жидкости", 150.0, 120),
                ("Кофе (стакан)",              "Напитки",           85.0,  200),
                ("Вода 0.5л",                  "Напитки",           60.0,  300),
                ("Шоколадный батончик",        "Снеки",             75.0,  150),
                ("Автомобильный освежитель",   "Аксессуары",        180.0, 60),
                ("Щётка стеклоочистителя 60", "Аксессуары",        450.0, 30),
            };

            foreach (var (name, cat, price, stock) in products)
            {
                using (var cmd = new SQLiteCommand(
                    "INSERT OR IGNORE INTO shop_products " +
                    "(name, category, price, stock, last_updated) " +
                    "VALUES (@name, @cat, @price, @stock, @date);", conn))
                {
                    cmd.Parameters.AddWithValue("@name",  name);
                    cmd.Parameters.AddWithValue("@cat",   cat);
                    cmd.Parameters.AddWithValue("@price", price);
                    cmd.Parameters.AddWithValue("@stock", stock);
                    cmd.Parameters.AddWithValue("@date",  DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                    cmd.ExecuteNonQuery();
                }
            }
        }
    }
}
