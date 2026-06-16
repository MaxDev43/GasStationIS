using System.Collections.Generic;

namespace GasStationIS.Models
{
    public class ShopSale
    {
        public int               Id            { get; set; }
        public string            SaleDatetime  { get; set; }
        public double            TotalAmount   { get; set; }
        public string            PaymentMethod { get; set; }
        public int?              EmployeeId    { get; set; }
        public string            EmployeeName  { get; set; }
        public List<ShopSaleItem> Items        { get; set; } = new List<ShopSaleItem>();
    }
}
