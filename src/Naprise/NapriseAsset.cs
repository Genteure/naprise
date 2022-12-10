using System.Collections.Generic;

namespace Naprise
{
    public class NapriseAsset
    {
        public Dictionary<MessageType, Color> NotificationTypeColor { get; set; } = new()
        {
            [MessageType.None] = "202225",
            [MessageType.Info] = "3AA3E3",
            [MessageType.Success] = "3AA337",
            [MessageType.Warning] = "CACF29",
            [MessageType.Error] = "A32037",
        };

        public Color GetColor(MessageType type) => this.NotificationTypeColor.TryGetValue(type, out var color) ? color : "202225";

        public Dictionary<MessageType, string> NotificationTypeAscii { get; set; } = new()
        {
            [MessageType.None] = string.Empty,
            [MessageType.Info] = "[i]",
            [MessageType.Success] = "[+]",
            [MessageType.Warning] = "[~]",
            [MessageType.Error] = "[!]",
        };

        public string GetAscii(MessageType type) => this.NotificationTypeAscii.TryGetValue(type, out var ascii) ? ascii : string.Empty;
    }
}
