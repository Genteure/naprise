using System.Text;
using System.Text.Json;

namespace Naprise.Json
{
    // TODO: use JsonNamingPolicy.SnakeCaseLower when that is available
    internal class JsonSnakeCaseNamingPolicy : JsonNamingPolicy
    {
        internal static readonly JsonSnakeCaseNamingPolicy Instance = new JsonSnakeCaseNamingPolicy();

        public override string ConvertName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return name;


            var b = new StringBuilder();
            for (var i = 0; i < name.Length; i++)
            {
                var c = name[i];
                if (char.IsUpper(c))
                {
                    if (i > 0)
                        b.Append('_');

                    b.Append(char.ToLowerInvariant(c));
                }
                else
                {
                    b.Append(c);
                }
            }

            return b.ToString();
        }
    }
}
