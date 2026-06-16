using System;
using System.Collections.Generic;
using GasStationIS.Core;
using GasStationIS.Models;

namespace GasStationIS.Services
{
    // ════════════════════════════════════════════════════════════════════
    //  ShopService — товары и продажи магазина при АЗС
    // ════════════════════════════════════════════════════════════════════
    public static class ShopService
    {
        // ── Products ──────────────────────────────────────────────────────
        public static List<ShopProduct> GetAllProducts() =>
            DatabaseService.ExecuteReader(
                "SELECT id, name, category, price, stock, barcode, last_updated " +
                "FROM shop_products ORDER BY category, name;",
                r => new ShopProduct
                {
                    Id          = Convert.ToInt32(r["id"]),
                    Name        = r["name"].ToString(),
                    Category    = r["category"]?.ToString(),
                    Price       = Convert.ToDouble(r["price"]),
                    Stock       = Convert.ToInt32(r["stock"]),
                    Barcode     = r["barcode"]?.ToString(),
                    LastUpdated = r["last_updated"].ToString(),
                });

        public static void AddProduct(ShopProduct p)
        {
            if (p == null) throw new ArgumentNullException(nameof(p));
            DatabaseService.ExecuteNonQuery(
                "INSERT INTO shop_products (name, category, price, stock, barcode, last_updated) " +
                "VALUES (@name, @cat, @price, @stock, @barcode, @date);",
                cmd =>
                {
                    cmd.Parameters.AddWithValue("@name",    p.Name);
                    cmd.Parameters.AddWithValue("@cat",     p.Category ?? "");
                    cmd.Parameters.AddWithValue("@price",   p.Price);
                    cmd.Parameters.AddWithValue("@stock",   p.Stock);
                    cmd.Parameters.AddWithValue("@barcode", p.Barcode ?? "");
                    cmd.Parameters.AddWithValue("@date",    DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                });
        }

        public static void UpdateProduct(ShopProduct p)
        {
            if (p == null) throw new ArgumentNullException(nameof(p));
            DatabaseService.ExecuteNonQuery(
                "UPDATE shop_products SET name=@name, category=@cat, price=@price, " +
                "stock=@stock, barcode=@barcode, last_updated=@date WHERE id=@id;",
                cmd =>
                {
                    cmd.Parameters.AddWithValue("@name",    p.Name);
                    cmd.Parameters.AddWithValue("@cat",     p.Category ?? "");
                    cmd.Parameters.AddWithValue("@price",   p.Price);
                    cmd.Parameters.AddWithValue("@stock",   p.Stock);
                    cmd.Parameters.AddWithValue("@barcode", p.Barcode ?? "");
                    cmd.Parameters.AddWithValue("@date",    DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                    cmd.Parameters.AddWithValue("@id",      p.Id);
                });
        }

        public static void DeleteProduct(int id) =>
            DatabaseService.ExecuteNonQuery(
                "DELETE FROM shop_products WHERE id=@id;",
                cmd => cmd.Parameters.AddWithValue("@id", id));

        // ── Sales ─────────────────────────────────────────────────────────
        public static List<ShopSale> GetAllSales() =>
            DatabaseService.ExecuteReader(
                "SELECT s.id, s.sale_datetime, s.total_amount, s.payment_method, " +
                "       s.employee_id, e.full_name AS emp_name " +
                "FROM shop_sales s " +
                "LEFT JOIN employees e ON e.id = s.employee_id " +
                "ORDER BY s.sale_datetime DESC;",
                r => new ShopSale
                {
                    Id            = Convert.ToInt32(r["id"]),
                    SaleDatetime  = r["sale_datetime"].ToString(),
                    TotalAmount   = Convert.ToDouble(r["total_amount"]),
                    PaymentMethod = r["payment_method"].ToString(),
                    EmployeeId    = r["employee_id"] == DBNull.Value ? (int?)null : Convert.ToInt32(r["employee_id"]),
                    EmployeeName  = r["emp_name"]?.ToString(),
                });

        public static double GetTodayRevenue()
        {
            var today  = DateTime.Now.ToString("yyyy-MM-dd");
            var result = DatabaseService.ExecuteScalar(
                "SELECT COALESCE(SUM(total_amount),0) FROM shop_sales WHERE sale_datetime LIKE @t||'%';",
                cmd => cmd.Parameters.AddWithValue("@t", today));
            return result == null || result == DBNull.Value ? 0 : Convert.ToDouble(result);
        }

        public static void AddSale(ShopSale sale, List<ShopSaleItem> items)
        {
            if (sale == null || items == null || items.Count == 0)
                throw new ArgumentException("Продажа и список товаров не могут быть пустыми.");

            using (var conn = DatabaseService.GetOpenConnection())
            using (var tx   = conn.BeginTransaction())
            {
                try
                {
                    // Заголовок чека
                    long saleId;
                    using (var cmd = new System.Data.SQLite.SQLiteCommand(
                        "INSERT INTO shop_sales (sale_datetime, total_amount, payment_method, employee_id) " +
                        "VALUES (@dt, @ta, @pm, @eid); SELECT last_insert_rowid();", conn, tx))
                    {
                        cmd.Parameters.AddWithValue("@dt",  sale.SaleDatetime ?? DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                        cmd.Parameters.AddWithValue("@ta",  sale.TotalAmount);
                        cmd.Parameters.AddWithValue("@pm",  sale.PaymentMethod ?? "Наличные");
                        cmd.Parameters.AddWithValue("@eid", (object)sale.EmployeeId ?? DBNull.Value);
                        saleId = (long)cmd.ExecuteScalar();
                    }

                    // Позиции чека + списание со склада
                    foreach (var item in items)
                    {
                        using (var cmd = new System.Data.SQLite.SQLiteCommand(
                            "INSERT INTO shop_sale_items (sale_id, product_id, product_name, quantity, price) " +
                            "VALUES (@sid, @pid, @pname, @qty, @price);", conn, tx))
                        {
                            cmd.Parameters.AddWithValue("@sid",   saleId);
                            cmd.Parameters.AddWithValue("@pid",   item.ProductId);
                            cmd.Parameters.AddWithValue("@pname", item.ProductName);
                            cmd.Parameters.AddWithValue("@qty",   item.Quantity);
                            cmd.Parameters.AddWithValue("@price", item.Price);
                            cmd.ExecuteNonQuery();
                        }

                        using (var cmd = new System.Data.SQLite.SQLiteCommand(
                            "UPDATE shop_products SET stock = stock - @qty WHERE id = @id;", conn, tx))
                        {
                            cmd.Parameters.AddWithValue("@qty", item.Quantity);
                            cmd.Parameters.AddWithValue("@id",  item.ProductId);
                            cmd.ExecuteNonQuery();
                        }
                    }

                    tx.Commit();
                    Logger.Info($"Продажа в магазине: {sale.TotalAmount:F2} руб., {items.Count} позиций");
                }
                catch
                {
                    tx.Rollback();
                    throw;
                }
            }
        }
    }
}
