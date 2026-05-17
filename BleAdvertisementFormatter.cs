using System.Text;

namespace controller;

internal static class BleAdvertisementFormatter
{
    public static string Format(byte[] payload, string deviceTypeLabel)
    {
        if (payload.Length < 8)
        {
            return "Advertisement payload is too short to decode.";
        }

        var lines = new StringBuilder();
        lines.AppendLine($"Device type: {deviceTypeLabel}");
        lines.AppendLine($"DVDD: {payload[0] * 0.1:F1} V");
        lines.AppendLine($"Battery A: {payload[1] * 0.1:F1} V");
        lines.AppendLine($"Battery B: {payload[2] * 0.1:F1} V");
        lines.AppendLine($"Impedance A: {payload[3] * 0.01:F2} V");
        lines.AppendLine($"Impedance B: {payload[4] * 0.01:F2} V");
        lines.AppendLine($"Thermistor ref: {payload[5] * 0.01:F2} V");
        lines.AppendLine($"Thermistor out: {payload[6] * 0.01:F2} V");
        lines.AppendLine($"Thermistor offset: {payload[7] * 0.01:F2} V");

        if (payload.Length > 8)
        {
            lines.AppendLine($"VRECT detected: {StatusBit(payload, 0)}");
            lines.AppendLine($"VRECT over-voltage: {StatusBit(payload, 1)}");
            lines.AppendLine($"Charger power-good: {StatusBit(payload, 2)}");
            lines.AppendLine($"CHG1 status: {StatusBit(payload, 3)}");
            lines.AppendLine($"CHG1 over-voltage error: {StatusBit(payload, 4)}");
            lines.AppendLine($"CHG2 status: {StatusBit(payload, 5)}");
            lines.AppendLine($"CHG2 over-voltage error: {StatusBit(payload, 6)}");
            lines.AppendLine($"Wireless charging active: {StatusBit(payload, 14)}");
            lines.AppendLine($"Wireless charging paused: {StatusBit(payload, 15)}");
            lines.AppendLine($"Temperature monitor enabled: {StatusBit(payload, 12)}");
            lines.AppendLine($"Impedance monitor enabled: {StatusBit(payload, 13)}");
        }

        if (payload.Length >= 23)
        {
            lines.AppendLine($"Hardware version byte: {payload[22]}");
        }

        var rawLength = Math.Min(payload.Length, 23);
        lines.Append("Raw payload: ");
        lines.AppendLine(Convert.ToHexString(payload.AsSpan(0, rawLength)));

        return lines.ToString().TrimEnd();
    }

    private static string StatusBit(byte[] payload, int bitFromMsd8)
    {
        var byteIndex = 8 + (bitFromMsd8 / 8);
        var bitPos = bitFromMsd8 % 8;
        if (byteIndex >= payload.Length)
        {
            return "n/a";
        }

        return (payload[byteIndex] & (1 << bitPos)) != 0 ? "yes" : "no";
    }
}
