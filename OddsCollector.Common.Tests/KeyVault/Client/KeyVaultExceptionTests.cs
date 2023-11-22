using FluentAssertions;
using OddsCollector.Common.KeyVault.Client;

namespace OddsCollector.Common.Tests.KeyVault.Client;

internal sealed class KeyVaultExceptionTests
{
    [Test]
    public void Constructor_WithoutParameters_HasValidProperties()
    {
        var exception = new KeyVaultException();

        exception.Should().NotBeNull();
        exception.KeyName.Should().BeNull();
        exception.InnerException.Should().BeNull();
    }

    [Test]
    public void Constructor_WithKey_HasValidProperties()
    {
        const string keyName = nameof(keyName);

        var exception = new KeyVaultException(keyName);

        exception.Should().NotBeNull();
        exception.KeyName.Should().Be(keyName);
        exception.InnerException.Should().BeNull();
    }

    [Test]
    public void Constructor_WithKeyAndMessage_HasValidProperties()
    {
        const string keyName = nameof(keyName);
        const string message = nameof(message);

        var exception = new KeyVaultException(keyName, message);

        exception.Should().NotBeNull();
        exception.KeyName.Should().Be(keyName);
        exception.Message.Should().Be(message);
        exception.InnerException.Should().BeNull();
    }

    [Test]
    public void Constructor_WithMessageAndException_HasValidProperties()
    {
        var innerException = new Exception();
        const string message = nameof(message);

        var exception = new KeyVaultException(message, innerException);

        exception.Should().NotBeNull();
        exception.KeyName.Should().BeNull();
        exception.Message.Should().Be(message);
        exception.InnerException.Should().NotBeNull().And.Be(innerException);
    }

    [Test]
    public void Constructor_WithKeyNameAndMessageAndException_HasValidProperties()
    {
        var innerException = new Exception();
        const string message = nameof(message);
        const string keyName = nameof(keyName);

        var exception = new KeyVaultException(keyName, message, innerException);

        exception.Should().NotBeNull();
        exception.KeyName.Should().Be(keyName);
        exception.Message.Should().Be(message);
        exception.InnerException.Should().NotBeNull().And.Be(innerException);
    }
}
