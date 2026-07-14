using System.Net;
using System.Text.RegularExpressions;

namespace PickupAPi.Utils
{
    public static class HtmlUtils
    {
        public static string Clean(string input) =>
            WebUtility.HtmlDecode(Regex.Replace(input ?? "", "<.*?>", "")).Trim();
    }
}