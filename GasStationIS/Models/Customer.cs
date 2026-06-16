namespace GasStationIS.Models
{
    public class Customer
    {
        public int    Id               { get; set; }
        public string FullName         { get; set; }
        public string Phone            { get; set; }
        public string Email            { get; set; }
        public string CarNumber        { get; set; }
        public double LoyaltyPoints    { get; set; }
        public double TotalSpent       { get; set; }
        public string RegistrationDate { get; set; }

        public string LoyaltyLevel =>
            TotalSpent >= 100_000 ? "Платиновый"
          : TotalSpent >= 50_000  ? "Золотой"
          : TotalSpent >= 10_000  ? "Серебряный"
                                   : "Базовый";
    }
}
