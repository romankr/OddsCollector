﻿using OddsCollector.Common.Models;

namespace OddsCollector.Common.OddsApi.Client;

public interface IOddsApiClient
{
    Task<IEnumerable<UpcomingEvent>> GetUpcomingEventsAsync(Guid traceId, DateTime timestamp);
    Task<IEnumerable<EventResult>> GetEventResultsAsync(Guid traceId, DateTime timestamp);
}
