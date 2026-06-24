namespace GasStationIS.Models
{
    // Клиент АЗС: ФИО, телефон и номер авто для привязки к продаже
    public class Customer
    {
        public int    Id               { get; set; }
        public string FullName         { get; set; }
        public string Phone            { get; set; }
        public string CarNumber        { get; set; }
        public string RegistrationDate { get; set; }
    }
}