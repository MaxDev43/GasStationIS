namespace GasStationIS.Models
{
    public class FuelSale
    {
        public int    Id            { get; set; }
        public int    FuelTypeId    { get; set; }
        public string FuelTypeName  { get; set; }
        public double Liters        { get; set; }
        public double PricePerLiter { get; set; }
        public double TotalAmount   { get; set; }
        public string SaleDatetime  { get; set; }
        public string PaymentMethod { get; set; }
        public int?   EmployeeId    { get; set; }
        public string EmployeeName  { get; set; }
        public int?   CustomerId    { get; set; }
        public string CustomerName  { get; set; }
    }
}
