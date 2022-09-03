﻿namespace OddsCollector.Api.GoogleApi;

using Betting;

/// <summary>
/// Provides access to Google API-s.
/// </summary>
public interface IGoogleApiAdapter
{
    /// <summary>
    /// Creates a new Google Sheets document with provided <see cref="BettingStrategyResult"/>.
    /// </summary>
    /// <param name="bettingStrategyName">A betting strategy name to use in the file name.</param>
    /// <param name="result">A <see cref="BettingStrategyResult"/> to be saved as a Google Sheets document.</param>
    /// <returns>String Id of the newly created Google Sheets document.</returns>
    Task<string> CreateReportAsync(string bettingStrategyName, BettingStrategyResult? result);
}