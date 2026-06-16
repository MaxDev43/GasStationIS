using System;

namespace GasStationIS.Models
{
    public class FuelSupply
    {
        public int    Id             { get; set; }
        public string SupplierName   { get; set; }
        public string SupplierPhone  { get; set; }
        public int    FuelTypeId     { get; set; }
        public string FuelTypeName   { get; set; }   // Денормализовано для отображения
        public double QuantityLiters { get; set; }
        public double PricePerLiter  { get; set; }
        public double TotalCost      { get; set; }
        public string SupplyDate     { get; set; }
        public int?   EmployeeId     { get; set; }
        public string EmployeeName   { get; set; }
        public string Notes          { get; set; }
    }
}
