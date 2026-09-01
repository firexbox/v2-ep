namespace ServiceLib.Handler.Fmt;

/// <summary>
///     encrypted-proxy (ep://) share link format:
///     ep://&lt;password&gt;@&lt;host&gt;:&lt;port&gt;?obfs=1&amp;jitter=1#remarks
///     Example: ep://mypassword@1.2.3.4:8388?obfs=1#my-server
/// </summary>
public class EpFmt : BaseFmt
{
    public static ProfileItem? Resolve(string str, out string msg)
    {
        msg = ResUI.ConfigurationFormatIncorrect;

        var parsedUrl = Utils.TryUri(str);
        if (parsedUrl == null)
        {
            return null;
        }

        ProfileItem item = new()
        {
            ConfigType = EConfigType.EncryptedProxy,
            Remarks = parsedUrl.GetComponents(UriComponents.Fragment, UriFormat.Unescaped),
            Address = parsedUrl.IdnHost,
            Port = parsedUrl.Port,
        };
        var rawUserInfo = Utils.UrlDecode(parsedUrl.UserInfo);
        item.Password = rawUserInfo;

        var query = Utils.ParseQueryString(parsedUrl.Query);
        var protocolExtra = item.GetProtocolExtra() with
        {
            Obfs = GetQueryValue(query, "obfs") == "1",
            Jitter = GetQueryValue(query, "jitter") == "1",
        };
        item.SetProtocolExtra(protocolExtra);

        return item;
    }

    public static string? ToUri(ProfileItem? item)
    {
        if (item == null)
        {
            return null;
        }
        var remark = string.Empty;
        if (item.Remarks.IsNotEmpty())
        {
            remark = "#" + Utils.UrlEncode(item.Remarks);
        }
        var pw = item.Password;
        var protocolExtra = item.GetProtocolExtra();
        var dicQuery = new Dictionary<string, string>();
        if (protocolExtra.Obfs == true)
        {
            dicQuery.Add("obfs", "1");
        }
        if (protocolExtra.Jitter == true)
        {
            dicQuery.Add("jitter", "1");
        }

        return ToUri(EConfigType.EncryptedProxy, item.Address, item.Port, pw, dicQuery, remark);
    }
}
