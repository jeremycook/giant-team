namespace WebApp.Charting
{
    public class ChartDataset
    {
        public string Label { get; set; } = null!;
        public string? BackgroundColor { get; set; }
        public string? BorderColor { get; set; }
        public List<object?> Data { get; set; } = new();
    }
}
