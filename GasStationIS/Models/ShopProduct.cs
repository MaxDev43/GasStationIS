namespace GasStationIS.Models
{
    public class ShopProduct
    {
        public int    Id          { get; set; }
        public string Name        { get; set; }
        public string Category    { get; set; }
        public double Price       { get; set; }
        public int    Stock       { get; set; }
        public string Barcode     { get; set; }
        public string LastUpdated { get; set; }
    }
}
