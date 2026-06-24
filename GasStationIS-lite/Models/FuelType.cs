// ═══════════════════════════════════════════════════════════════════════
//  Models/FuelType.cs
// ═══════════════════════════════════════════════════════════════════════
namespace GasStationIS.Models
{
    public class FuelType
    {
        public int    Id           { get; set; }
        public string Name         { get; set; }
        public double PricePerLiter{ get; set; }
        public double StockLiters  { get; set; }
        public double TankCapacity { get; set; }
        public string LastUpdated  { get; set; }

        /// <summary>Процент заполненности резервуара (0–100).</summary>
        public double StockPercent =>
            TankCapacity > 0 ? (StockLiters / TankCapacity) * 100.0 : 0;

        public string StockStatus =>
            StockPercent < 15 ? "Критически мало"
          : StockPercent < 30 ? "Мало"
          : StockPercent < 70 ? "Норма"
                               : "Полный";
    }
}
