namespace ServiceLib.Tests.Fmt;

public class EpFmtTests
{
    [Test]
    public async Task ResolveConfig_EpUri_ShouldParseAllFields()
    {
        var resolved = FmtHandler.ResolveConfig("ep://mypassword@1.2.3.4:8388?obfs=1&jitter=1#my-server", out var msg);

        await resolved.Should().NotBeNull().Because($"msg: {msg}");
        await resolved!.ConfigType.Should().BeEqualTo(EConfigType.EncryptedProxy);
        await resolved.Address.Should().BeEqualTo("1.2.3.4");
        await resolved.Port.Should().BeEqualTo(8388);
        await resolved.Password.Should().BeEqualTo("mypassword");
        await resolved.Remarks.Should().BeEqualTo("my-server");
        await resolved.GetProtocolExtra().Obfs.Should().BeTrue();
        await resolved.GetProtocolExtra().Jitter.Should().BeTrue();
    }

    [Test]
    public async Task ResolveConfig_EpUri_Defaults_ShouldBeNoObfsNoJitter()
    {
        var resolved = FmtHandler.ResolveConfig("ep://pass@example.com:443", out _);

        await resolved.Should().NotBeNull();
        await resolved!.GetProtocolExtra().Obfs.Should().BeFalse();
        await resolved.GetProtocolExtra().Jitter.Should().BeFalse();
        await resolved.IsValid().Should().BeTrue();
    }

    [Test]
    public async Task ToUri_EpProfile_ShouldRoundTrip()
    {
        var resolved = FmtHandler.ResolveConfig("ep://p%40ss@example.com:8388?obfs=1#remark", out _);
        await resolved.Should().NotBeNull();

        var uri = FmtHandler.GetShareUri(resolved!);

        await uri.Should().NotBeNull();
        await uri.Should().NotBeEmpty();
        var resolved2 = FmtHandler.ResolveConfig(uri!, out var msg2);
        await resolved2.Should().NotBeNull().Because($"uri: {uri}, msg: {msg2}");
        await resolved2!.Address.Should().BeEqualTo("example.com");
        await resolved2.Port.Should().BeEqualTo(8388);
        await resolved2.Password.Should().BeEqualTo("p@ss");
        await resolved2.GetProtocolExtra().Obfs.Should().BeTrue();
        await resolved2.Remarks.Should().BeEqualTo("remark");
    }

    [Test]
    public async Task ResolveConfig_NonEpUri_ShouldBeNull()
    {
        var resolved = FmtHandler.ResolveConfig("vless://uuid@example.com:443", out _);

        await resolved.Should().NotBeNull(); // vless is still a valid protocol, but not EP
        await resolved!.ConfigType.Should().NotBeEqualTo(EConfigType.EncryptedProxy);
    }
}
