namespace OddsCollector.OddsApiService.Client;

internal class DefaultParameters : IDefaultParameters
{
    public DateFormat GetDateFormat()
    {
        return DateFormat.Iso;
    }

    public Markets GetMarkets()
    {
        return Markets.H2h;
    }

    public Markets2Key GetMarkets2Key()
    {
        return Markets2Key.H2h;
    }

    public OddsFormat GetOddsFormat()
    {
        return OddsFormat.Decimal;
    }

    public Regions GetRegions()
    {
        return Regions.Eu;
    }

    public int GetDaysFrom()
    {
        return 3;
    }
}
