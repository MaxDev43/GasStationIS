namespace GasStationIS.Models
{
    /// <summary>Строка отчёта — используется в разделе Отчёты.</summary>
    public class ReportEntry
    {
        public string Label  { get; set; }
        public double Value  { get; set; }
        public string Detail { get; set; }
    }
}
