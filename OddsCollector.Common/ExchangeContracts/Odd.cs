namespace OddsCollector.Common.ExchangeContracts;

public class Odd
{
    public string? Bookmaker { get; set; }
    public double? Home { get; set; }
    public double? Draw { get; set; }
    public double? Away { get; set; }
}