namespace OddsCollector.Common.Models;

public class OddBuilder
{
    public Odd Instance { get; } = new();

    public OddBuilder SetBookmaker(string? bookmaker)
    {
        if (string.IsNullOrEmpty(bookmaker))
        {
            throw new ArgumentException($"{nameof(bookmaker)} cannot be null or empty", nameof(bookmaker));
        }

        Instance.Bookmaker = bookmaker;

        return this;
    }

    public OddBuilder SetAway(double? away)
    {
        if (away is null)
        {
            throw new ArgumentNullException(nameof(away));
        }

        Instance.Away = away.Value;

        return this;
    }

    public OddBuilder SetDraw(double? draw)
    {
        if (draw is null)
        {
            throw new ArgumentNullException(nameof(draw));
        }

        Instance.Draw = draw.Value;

        return this;
    }

    public OddBuilder SetHome(double? home)
    {
        if (home is null)
        {
            throw new ArgumentNullException(nameof(home));
        }

        Instance.Home = home.Value;

        return this;
    }
}
