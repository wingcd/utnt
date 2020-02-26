using System;

namespace HttpServer.Tools
{
    public class HttpUtility
    {
        public static string UrlDecode(string url)
        {
            return Uri.UnescapeDataString(url);
        }

        public static string UrlEncode(string url)
        {
            return Uri.EscapeDataString(url);
        }
    }
}