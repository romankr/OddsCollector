﻿namespace OddsCollector.Common.ExchangeContracts;

public class EventResult
{
    public DateTime? CommenceTime { get; init; }
    public string? Id { get; init; }
    public DateTime? Timestamp { get; init; }
    public Guid? TraceId { get; init; }
    public string? Winner { get; init; }
}
