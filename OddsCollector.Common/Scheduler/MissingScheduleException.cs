namespace OddsCollector.Common.Scheduler;

using System;

public class MissingScheduleException : Exception
{
    public MissingScheduleException()
    {
    }

    public MissingScheduleException(string? message)
        : base(message)
    {
    }

    public MissingScheduleException(string? message, Exception? inner)
        : base(message, inner)
    {
    }
}