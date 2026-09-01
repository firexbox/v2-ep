using System.Text.Json;
using System.Text.Json.Nodes;

namespace ServiceLib.Services.CoreConfig;

/// <summary>
///     Generates the encrypted-proxy client config (config.json).
///     The encrypted-proxy Go core reads: mode/listen/http-listen/remote/password/obfs/jitter/pool-size.
///     The "inbounds" array is informational (v2rayN reads local ports from its own settings).
/// </summary>
public class CoreConfigEpService(CoreConfigContext context)
{
    private static readonly string _tag = "CoreConfigEpService";
    private readonly Config _config = context.AppConfig;
    private readonly ProfileItem _node = context.Node;

    public RetResult GenerateClientConfigContent()
    {
        var socksPort = AppManager.Instance.GetLocalPort(EInboundProtocol.socks);
        return GenerateConfig(socksPort, socksPort + 1);
    }

    /// <summary>
    ///     Generates a speed-test config: the encrypted-proxy client listens on the given
    ///     speed-test port (single server, no extra HTTP listener).
    /// </summary>
    public RetResult GenerateSpeedtestConfig(int port)
    {
        return GenerateConfig(port, 0);
    }

    private RetResult GenerateConfig(int listenPort, int httpPort)
    {
        var ret = new RetResult();
        try
        {
            if (_node == null || !_node.IsValid())
            {
                ret.Msg = ResUI.CheckServerSettings;
                return ret;
            }

            var protocolExtra = _node.GetProtocolExtra();

            var root = new JsonObject
            {
                ["mode"] = "client",
                ["listen"] = $"127.0.0.1:{listenPort}",
                ["remote"] = $"{_node.Address}:{_node.Port}",
                ["password"] = _node.Password,
                ["obfs"] = protocolExtra.Obfs ?? true,
                ["jitter"] = protocolExtra.Jitter ?? false,
                ["pool-size"] = protocolExtra.PoolSize ?? 5,
            };
            if (httpPort > 0)
            {
                root["http-listen"] = $"127.0.0.1:{httpPort}";
            }
            root["inbounds"] = new JsonArray
            {
                new JsonObject
                {
                    ["protocol"] = "socks",
                    ["listen"] = "127.0.0.1",
                    ["port"] = listenPort,
                },
            };

            ret.Data = JsonUtils.Serialize(root, new JsonSerializerOptions
            {
                WriteIndented = true,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            });
            ret.Msg = string.Format(ResUI.SuccessfulConfiguration, "");
            ret.Success = true;
        }
        catch (Exception ex)
        {
            Logging.SaveLog(_tag, ex);
            ret.Msg = ResUI.FailedGenDefaultConfiguration;
        }
        return ret;
    }
}
