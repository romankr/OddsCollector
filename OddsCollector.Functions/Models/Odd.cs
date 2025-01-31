namespace OddsCollector.Functions.Models;

internal sealed class Odd
{
    public double Away { get; set; }
    public string Bookmaker { get; set; } = string.Empty;
    public double Draw { get; set; }
    public double Home { get; set; }
}
