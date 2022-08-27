namespace OddsCollector.Prediction;

public class Prediction
{
    public string? SportEventId { get; set; }

    public string? AwayTeam { get; set; }

    public string? HomeTeam { get; set; }

    public string? BestBookmaker { get; set; }

    public double BestScore { get; set; }

    public string? ExpectedOutcome  { get; set; }

    public string? RealOutcome { get; set; }
}