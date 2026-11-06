using FluentAssertions.Execution;
using Microsoft.Extensions.Diagnostics.Metrics.Testing;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Logging.Testing;
using OddsCollector.Functions.Functions;
using OddsCollector.Functions.ServiceBus;
using OddsCollector.Functions.UnitTests.Infrastructure.Metrics;
using OddsCollector.Functions.UnitTests.Infrastructure.ServiceBus;

namespace OddsCollector.Functions.UnitTests.Tests.Functions;

internal sealed class DeadLetterFunctionTests
{
    private const string MessageId = "messageId";
    private const string Reason = "JsonException";
    private const string Description = "Message body is null";
    private const string Body = """{"Id":"eventId"}""";

    private static readonly DeadLetterMetrics UnobservedMetrics = new(new TestMeterFactory());

    [Test]
    public void Run_WithDeadLetteredMessage_LogsItWithReasonDescriptionAndBody()
    {
        // Arrange
        var loggerMock = new FakeLogger<DeadLetterFunction>();

        var function = CreateFunction(loggerMock);

        var message = ServiceBusReceivedMessageFactory.CreateDeadLettered(Body, MessageId, Reason, Description);

        // Act
        function.Run(message);

        // Assert
        loggerMock.Collector.GetSnapshot().Should().ContainSingle().Which.Should().BeEquivalentTo(new
        {
            Level = LogLevel.Error,
            Message = $"Removed dead-lettered message {MessageId}: {Reason}: {Description}. Body: {Body}"
        });
    }

    [Test]
    public void Run_WithDeadLetteredMessage_RecordsItWithItsReason()
    {
        // Arrange
        using var meterFactory = new TestMeterFactory();
        using var deadLetteredMock = new MetricCollector<long>(meterFactory, DeadLetterMetrics.MeterName,
            DeadLetterMetrics.DeadLetteredMessagesInstrumentName);

        var function = CreateFunction(metrics: new DeadLetterMetrics(meterFactory));

        var message = ServiceBusReceivedMessageFactory.CreateDeadLettered(Body, MessageId, Reason, Description);

        // Act
        function.Run(message);

        // Assert
        var measurement = deadLetteredMock.GetMeasurementSnapshot().Should().ContainSingle().Subject;

        using var scope = new AssertionScope();

        measurement.Value.Should().Be(1);
        measurement.Tags.Should().BeEquivalentTo(new Dictionary<string, object?>
        {
            [DeadLetterMetrics.ReasonTag] = Reason
        });
    }

    [TestCase(null, TestName = "Run_WithoutDeadLetterReason_RecordsUnknownReason")]
    [TestCase(" ", TestName = "Run_WithBlankDeadLetterReason_RecordsUnknownReason")]
    public void Run_WithoutUsableDeadLetterReason_RecordsUnknownReason(string? reason)
    {
        // Arrange
        using var meterFactory = new TestMeterFactory();
        using var deadLetteredMock = new MetricCollector<long>(meterFactory, DeadLetterMetrics.MeterName,
            DeadLetterMetrics.DeadLetteredMessagesInstrumentName);

        var function = CreateFunction(metrics: new DeadLetterMetrics(meterFactory));

        var message = ServiceBusReceivedMessageFactory.CreateDeadLettered(Body, MessageId, reason, null);

        // Act
        function.Run(message);

        // Assert
        deadLetteredMock.GetMeasurementSnapshot().Should().ContainSingle().Which.Tags.Should()
            .BeEquivalentTo(new Dictionary<string, object?>
            {
                [DeadLetterMetrics.ReasonTag] = DeadLetterFunction.UnknownReason
            });
    }

    [Test]
    public void Run_WithBodyLongerThanLogged_LogsOnlyTheStartOfTheBody()
    {
        // Arrange: one character more than is logged.
        var longBody = new string('a', DeadLetterFunction.MaxLoggedBodyLength + 1);
        var loggedBody = $"{new string('a', DeadLetterFunction.MaxLoggedBodyLength)}…";

        var loggerMock = new FakeLogger<DeadLetterFunction>();

        var function = CreateFunction(loggerMock);

        var message = ServiceBusReceivedMessageFactory.CreateDeadLettered(longBody, MessageId, Reason, Description);

        // Act
        function.Run(message);

        // Assert
        loggerMock.Collector.GetSnapshot().Should().ContainSingle().Which.Message.Should()
            .EndWith($"Body: {loggedBody}");
    }

    [Test]
    public void Run_WithNullMessage_Throws()
    {
        // Arrange
        var function = CreateFunction();

        // Act
        var action = () => function.Run(null!);

        // Assert
        action.Should().ThrowExactly<ArgumentNullException>().WithParameterName("message");
    }

    private static DeadLetterFunction CreateFunction(ILogger<DeadLetterFunction>? logger = null,
        DeadLetterMetrics? metrics = null)
    {
        return new DeadLetterFunction(logger ?? NullLogger<DeadLetterFunction>.Instance,
            metrics ?? UnobservedMetrics);
    }
}
