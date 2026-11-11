namespace OddsCollector.Functions.OddsApi.Converters;

// The result of converting a record of The Odds API: the converted value, or why the record was left out.
// The API often has gaps (no price, a missing score, no head-to-head market), so leaving a record out is an
// expected result rather than an exception. Exceptions stay for programming errors.
internal union Conversion<T>(T, Rejection);
