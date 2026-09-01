namespace ServiceLib.Tests.CoreConfig.Ep;

public class CoreConfigEpServiceTests
{
    [Test]
    public async Task GenerateClientConfigContent_ShouldEmitEpClientSettings()
    {
        var config = CoreConfigTestFactory.CreateConfig(ECoreType.encryptedproxy);
        CoreConfigTestFactory.BindAppManagerConfig(config);
        var node = CoreConfigTestFactory.CreateEpNode(ECoreType.encryptedproxy);
        var context = CoreConfigTestFactory.CreateContext(config, node, ECoreType.encryptedproxy);

        var result = new CoreConfigEpService(context).GenerateClientConfigContent();

        await result.Success.Should().BeTrue();
        await result.Data.Should().NotBeNull();

        var root = JsonNode.Parse(result.Data!.ToString());
        await root.Should().NotBeNull();
        await root!["mode"]!.GetValue<string>().Should().BeEqualTo("client");
        await root["listen"]!.GetValue<string>().Should().BeEqualTo("127.0.0.1:10808");
        await root["http-listen"]!.GetValue<string>().Should().BeEqualTo("127.0.0.1:10809");
        await root["remote"]!.GetValue<string>().Should().BeEqualTo("ep.example.com:8388");
        await root["password"]!.GetValue<string>().Should().BeEqualTo("ep-password");
        await root["obfs"]!.GetValue<bool>().Should().BeTrue();
        await root["jitter"]!.GetValue<bool>().Should().BeFalse();
        await root["pool-size"]!.GetValue<int>().Should().BeEqualTo(5);
        await root["inbounds"]!.AsArray().Count.Should().BeEqualTo(2);
    }

    [Test]
    public async Task GenerateClientConfig_WithEpCore_ShouldDispatchToEpService()
    {
        var config = CoreConfigTestFactory.CreateConfig(ECoreType.encryptedproxy);
        CoreConfigTestFactory.BindAppManagerConfig(config);
        var node = CoreConfigTestFactory.CreateEpNode(ECoreType.encryptedproxy);
        var context = CoreConfigTestFactory.CreateContext(config, node, ECoreType.encryptedproxy);

        var fileName = Path.GetTempFileName();
        try
        {
            var result = await CoreConfigHandler.GenerateClientConfig(context, fileName);

            await result.Success.Should().BeTrue();
            var fileContent = await File.ReadAllTextAsync(fileName);
            var root = JsonNode.Parse(fileContent);
            await root.Should().NotBeNull();
            await root!["mode"]!.GetValue<string>().Should().BeEqualTo("client");
            await root["remote"]!.GetValue<string>().Should().BeEqualTo("ep.example.com:8388");
        }
        finally
        {
            File.Delete(fileName);
        }
    }

    [Test]
    public async Task NodeValidator_ShouldRejectEpNodeWithoutPassword()
    {
        var node = CoreConfigTestFactory.CreateEpNode(ECoreType.encryptedproxy);
        node.Password = string.Empty;

        var validatorResult = NodeValidator.Validate(node, ECoreType.encryptedproxy);

        await validatorResult.Success.Should().BeFalse();
        await validatorResult.Errors.Should().NotBeEmpty();
    }
}
