namespace OddsCollector.OddsApiService.Client;

internal interface IDefaultParameters
{
    Regions GetRegions();
    Markets GetMarkets();
    DateFormat GetDateFormat();
    OddsFormat GetOddsFormat();
    Markets2Key GetMarkets2Key();
    int GetDaysFrom();
}
