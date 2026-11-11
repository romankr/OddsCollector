namespace OddsCollector.Functions.OddsApi.Converters;

// Why a record of The Odds API was left out. The reason is logged after the record's name, so it is a lowercase
// phrase: "Skipped event {Id}: {Reason}".
internal sealed record Rejection(string Reason);
