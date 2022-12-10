using Flurl;
using System;
using System.Linq;

namespace Naprise
{
    public static class QueryParamsExtensions
    {
        public static string? GetString(this QueryParamCollection query, string key, string? defaultValue = null)
        {
            if (!query.TryGetFirst(key, out var value))
                return defaultValue;

            if (value is string str)
                return str;

            return value?.ToString();
        }

        public static string[] GetStringArray(this QueryParamCollection query, string key)
            => query.GetAll(key)
                    .SelectMany(x => (x as string)?.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries) ?? Array.Empty<string>())
                    .ToArray();

        public static int? GetInt(this QueryParamCollection query, string key, int? defaultValue = null)
        {
            if (!query.TryGetFirst(key, out var value))
                return defaultValue;

            if (value is int i)
                return i;

            return int.TryParse(value as string, out var x) ? x : defaultValue;
        }

        public static bool? GetBool(this QueryParamCollection query, string key, bool? defaultValue = null)
        {
            if (!query.TryGetFirst(key, out var value))
                return defaultValue;

            if (value is null)
                return true;

            if (value is bool b)
                return b;

            if (value is not string str)
                return defaultValue;

            return str.ToLowerInvariant() switch
            {
                "false" or "0" or "no" => false,
                _ => true,
            };
        }
    }
}
