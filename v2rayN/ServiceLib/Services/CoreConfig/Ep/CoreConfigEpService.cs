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
        var ret = new RetResult();
        try
        {
            if (_node == null || !_node.IsValid())
            {
                ret.Msg = ResUI.CheckServerSettings;
                return ret;
            }

            var socksPort = AppManager.Instance.GetLocalPort(EInboundProtocol.socks);
            var httpPort = socksPort + 1;
            var protocolExtra = _node.GetProtocolExtra();

            var root = new JsonObject
            {
                ["mode"] = "client",
                ["listen"] = $"127.0.0.1:{socksPort}",
                ["http-listen"] = $"127.0.0.1:{httpPort}",
                ["remote"] = $"{_node.Address}:{_node.Port}",
                ["password"] = _node.Password,
                ["obfs"] = protocolExtra.Obfs ?? true,
                ["jitter"] = protocolExtra.Jitter ?? false,
                ["pool-size"] = protocolExtra.PoolSize ?? 5,
                ["inbounds"] = new JsonArray
                {
                    new JsonObject
                    {
                        ["protocol"] = "socks",
                        ["listen"] = "127.0.0.1",
                        ["port"] = socksPort,
                    },
                    new JsonObject
                    {
                        ["protocol"] = "http",
                        ["listen"] = "127.0.0.1",
                        ["port"] = httpPort,
                    },
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
