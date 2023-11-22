using Azure;
using Azure.Security.KeyVault.Secrets;
using FluentAssertions;
using NSubstitute;
using OddsCollector.Common.KeyVault.Client;

namespace OddsCollector.Common.Tests.KeyVault.Client;

internal sealed class KeyVaultClientTests
{
    [Test]
    public void Constructor_WithValidDependencies_ReturnsNewInstance()
    {
        var secretClient = Substitute.For<SecretClient>();

        var result = new KeyVaultClient(secretClient);

        result.Should().NotBeNull();
    }

    [Test]
    public void Constructor_WithNullSecretClient_ThrowsException()
    {
        var action = () =>
        {
            _ = new KeyVaultClient(null);
        };

        action.Should().Throw<ArgumentNullException>().WithParameterName("client");
    }

    [Test]
    public async Task GetOddsApiKey_WithValidSecretInKeyVault_ReturnsValidValue()
    {
        const string keyName = "OddsApiKey";
        const string expected = nameof(expected);
        var secret = new KeyVaultSecret(keyName, expected);

        var responseStub = Substitute.For<Response<KeyVaultSecret>>();
        responseStub.Value.Returns(secret);

        var secretClientStub = Substitute.For<SecretClient>();
        secretClientStub.GetSecretAsync(Arg.Any<string>()).Returns(Task.FromResult(responseStub));

        var client = new KeyVaultClient(secretClientStub);

        var result = await client.GetOddsApiKey();

        result.Should().NotBeNull().And.Be(expected);
    }

    [Test]
    public void GetOddsApiKey_WithEmptySecretInKeyVault_ThrowsException()
    {
        const string keyName = "OddsApiKey";
        var secret = new KeyVaultSecret(keyName, string.Empty);

        var responseStub = Substitute.For<Response<KeyVaultSecret>>();
        responseStub.Value.Returns(secret);

        var stub = Substitute.For<SecretClient>();
        stub.GetSecretAsync(Arg.Any<string>()).Returns(Task.FromResult(responseStub));

        var client = new KeyVaultClient(stub);

        Action action = () => client.GetOddsApiKey().GetAwaiter().GetResult();

        action.Should().Throw<KeyVaultException>().Which.KeyName.Should().Be(keyName);
    }

    [Test]
    public void GetOddsApiKey_WithNullResponse_ThrowsException()
    {
        const string keyName = "OddsApiKey";

        var responseStub = Substitute.For<Response<KeyVaultSecret>>();
        responseStub.Value.Returns(null as KeyVaultSecret);

        var stub = Substitute.For<SecretClient>();
        stub.GetSecretAsync(Arg.Any<string>()).Returns(Task.FromResult(responseStub));

        var client = new KeyVaultClient(stub);

        Action action = () => client.GetOddsApiKey().GetAwaiter().GetResult();

        action.Should().Throw<KeyVaultException>().Which.KeyName.Should().Be(keyName);
    }
}
