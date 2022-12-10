using System;
using System.Globalization;

namespace Naprise
{
    public sealed class Color
    {
        public Color(int value)
        {
            this.Value = value;
        }

        public Color(byte red, byte green, byte blue)
        {
            this.Value = (red << 16) | (green << 8) | blue;
        }

        public Color(string hex)
        {
            if (hex.StartsWith("#"))
                hex = hex.Substring(1);

            if (hex.Length == 3)
                hex = string.Format("{0}{0}{1}{1}{2}{2}", hex[0], hex[1], hex[2]);

            if (hex.Length != 6)
                throw new ArgumentException("Invalid hex color", "hex");

            this.Value = int.Parse(hex, NumberStyles.HexNumber);
        }

        public int Value { get; set; }

        public byte R
        {
            get => (byte)(this.Value >> 16);
            set => this.Value = (this.Value & 0x00FFFF) | (value << 16);
        }

        public byte G
        {
            get => (byte)(this.Value >> 8);
            set => this.Value = (this.Value & 0xFF00FF) | (value << 8);
        }

        public byte B
        {
            get => (byte)this.Value;
            set => this.Value = (this.Value & 0xFFFF00) | (value);
        }

        public string Hex
        {
            get => $"#{this.R:X2}{this.G:X2}{this.B:X2}";
            set
            {
                if (value.StartsWith("#"))
                {
                    value = value.Substring(1);
                }

                this.R = byte.Parse(value.Substring(0, 2), NumberStyles.HexNumber);
                this.G = byte.Parse(value.Substring(2, 2), NumberStyles.HexNumber);
                this.B = byte.Parse(value.Substring(4, 2), NumberStyles.HexNumber);
            }
        }

        public static implicit operator Color(int value)
        {
            return new Color(value);
        }

        public static implicit operator int(Color color)
        {
            return color.Value;
        }

        public static implicit operator Color(string hex)
        {
            return new Color(hex);
        }

        public static implicit operator string(Color color)
        {
            return color.Hex;
        }
    }
}
