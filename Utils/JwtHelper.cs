using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace PickupAPi.Utils
{
    public static class JwtHelper
    {
        private const string Secret = "Rng09av8YqGItWQxnd9hp_wQ7xUEdP-lV71zeKTN9oM";

        public static string Generate(Dictionary<string, object> claims)
        {
            string header = Base64UrlEncode(Encoding.UTF8.GetBytes("{\"alg\":\"HS256\",\"typ\":\"JWT\"}"));
            string payload = Base64UrlEncode(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(claims)));
            string signingInput = header + "." + payload;

            byte[] signature;
            using (var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(Secret)))
            {
                signature = hmac.ComputeHash(Encoding.UTF8.GetBytes(signingInput));
            }

            return signingInput + "." + Base64UrlEncode(signature);
        }

        public static Dictionary<string, object> Validate(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
                return null;

            string[] parts = token.Split('.');
            if (parts.Length != 3)
                return null;

            string signingInput = parts[0] + "." + parts[1];
            byte[] expected;
            using (var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(Secret)))
            {
                expected = hmac.ComputeHash(Encoding.UTF8.GetBytes(signingInput));
            }

            if (!SlowEquals(Base64UrlEncode(expected), parts[2]))
                return null;

            try
            {
                string json = Encoding.UTF8.GetString(Base64UrlDecode(parts[1]));
                var claims = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
                if (claims == null)
                    return null;

                if (claims.TryGetValue("exp", out object expObj))
                {
                    long exp = Convert.ToInt64(expObj);
                    if (DateTimeOffset.UtcNow.ToUnixTimeSeconds() >= exp)
                        return null;
                }

                return claims;
            }
            catch
            {
                return null;
            }
        }

        public static Dictionary<string, string> GetWorkspaces(Dictionary<string, object> claims)
        {
            var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            if (claims != null && claims.TryGetValue("workspaces", out object wsObj) && wsObj is JObject jObj)
            {
                foreach (var prop in jObj.Properties())
                    result[prop.Name] = prop.Value.ToString();
            }
            return result;
        }

        private static string Base64UrlEncode(byte[] input)
        {
            return Convert.ToBase64String(input)
                .TrimEnd('=')
                .Replace('+', '-')
                .Replace('/', '_');
        }

        private static byte[] Base64UrlDecode(string input)
        {
            string s = input.Replace('-', '+').Replace('_', '/');
            switch (s.Length % 4)
            {
                case 2: s += "=="; break;
                case 3: s += "="; break;
            }
            return Convert.FromBase64String(s);
        }

        private static bool SlowEquals(string a, string b)
        {
            if (a == null || b == null || a.Length != b.Length)
                return false;
            int diff = 0;
            for (int i = 0; i < a.Length; i++)
                diff |= a[i] ^ b[i];
            return diff == 0;
        }
    }
}
