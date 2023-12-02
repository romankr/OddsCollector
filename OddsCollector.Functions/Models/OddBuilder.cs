namespace OddsCollector.Functions.Models;

internal class OddBuilder
{
    public Odd Instance { get; } = new();

    public OddBuilder SetBookmaker(string? bookmaker)
    {
        ArgumentException.ThrowIfNullOrEmpty(bookmaker);

        Instance.Bookmaker = bookmaker;

        return this;
    }

    public OddBuilder SetAway(double? away)
    {
        ArgumentNullException.ThrowIfNull(away);

        Instance.Away = away.Value;

        return this;
    }

    public OddBuilder SetDraw(double? draw)
    {
        ArgumentNullException.ThrowIfNull(draw);

        Instance.Draw = draw.Value;

        return this;
    }

    public OddBuilder SetHome(double? home)
    {
        ArgumentNullException.ThrowIfNull(home);

        Instance.Home = home.Value;

        return this;
    }
}
