namespace GasStationIS.Models
{
    public class ShopSaleItem
    {
        public int    Id          { get; set; }
        public int    SaleId      { get; set; }
        public int    ProductId   { get; set; }
        public string ProductName { get; set; }
        public int    Quantity    { get; set; }
        public double Price       { get; set; }
        public double Total       => Quantity * Price;
    }
}
