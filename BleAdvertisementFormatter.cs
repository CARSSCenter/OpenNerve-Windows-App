using System.Text;

namespace controller;

internal static class BleAdvertisementFormatter
{
    public static string CsvHeader() =>
        "Timestamp,Device,Type,DVDD_V,BattA_V,BattB_V,ImpA_V,ImpB_V,Temp_C," +
        "CoilPresent,CHG1_Charging,CHG2_Charging,WirelessActive,WirelessPaused,HW_Version,Notes";

    public static string ToCsvRow(byte[] payload, string deviceType, string mac, DateTime ts, string notes = "")
    {
        string tempStr = payload.Length >= 8
            ? (CalcTemperature(payload[5] * 10.0, payload[6] * 10.0, payload[7] * 10.0)?.ToString("F1") ?? "n/a")
            : "n/a";

        string Bit(int bit, bool invert = false) => payload.Length > 8 ? StatusBit(payload, bit, invert) : "n/a";

        string hwVer = payload.Length >= 23 ? payload[22].ToString() : "n/a";

        return string.Join(",",
            ts.ToString("yyyy-MM-dd HH:mm:ss.fff"),
            mac,
            deviceType,
            payload.Length > 0 ? (payload[0] * 0.1).ToString("F1") : "",
            payload.Length > 1 ? (payload[1] * 0.1).ToString("F1") : "",
            payload.Length > 2 ? (payload[2] * 0.1).ToString("F1") : "",
            payload.Length > 3 ? (payload[3] * 0.01).ToString("F2") : "",
            payload.Length > 4 ? (payload[4] * 0.01).ToString("F2") : "",
            tempStr,
            Bit(0, invert: true),
            Bit(3, invert: true),
            Bit(5, invert: true),
            Bit(14),
            Bit(15),
            hwVer,
            notes);
    }

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
        double? tempC = CalcTemperature(payload[5] * 10.0, payload[6] * 10.0, payload[7] * 10.0);
        lines.AppendLine(tempC.HasValue ? $"Temperature: {tempC.Value:F1} °C" : "Temperature: n/a");

        if (payload.Length > 8)
        {
            lines.AppendLine($"Coil present: {StatusBit(payload, 0, invert: true)}");
            lines.AppendLine($"VRECT over-voltage fault: {StatusBit(payload, 1, invert: true)}");
            lines.AppendLine($"Charger power-good: {StatusBit(payload, 2)}");
            lines.AppendLine($"CHG1 charging: {StatusBit(payload, 3, invert: true)}");
            lines.AppendLine($"CHG1 over-voltage error: {StatusBit(payload, 4, invert: true)}");
            lines.AppendLine($"CHG2 charging: {StatusBit(payload, 5, invert: true)}");
            lines.AppendLine($"CHG2 over-voltage error: {StatusBit(payload, 6, invert: true)}");
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

    // 104AP-2 NTC: (°C, Ω) pairs from the WPT firmware lookup table
    private static readonly (double TempC, double Ohms)[] ThermTable =
    [
        (20, 126_400),
        (25, 100_000),
        (30,  79_590),
        (40,  51_320),
        (50,  33_790),
    ];

    private static double? CalcTemperature(double refMv, double outMv, double ofstMv)
    {
        double voltage = outMv - ofstMv;
        double voltageDrop = refMv - outMv;
        if (voltage <= 0 || voltageDrop <= 0)
            return null;

        double resistance = voltage * 49_900.0 / voltageDrop;

        if (resistance >= ThermTable[0].Ohms)
            return ThermTable[0].TempC;
        if (resistance <= ThermTable[^1].Ohms)
            return ThermTable[^1].TempC;

        for (int i = 0; i < ThermTable.Length - 1; i++)
        {
            double rHi = ThermTable[i].Ohms;
            double rLo = ThermTable[i + 1].Ohms;
            if (resistance <= rHi && resistance >= rLo)
            {
                double frac = (rHi - resistance) / (rHi - rLo);
                return ThermTable[i].TempC + frac * (ThermTable[i + 1].TempC - ThermTable[i].TempC);
            }
        }

        return null;
    }

    private static string StatusBit(byte[] payload, int bitFromMsd8, bool invert = false)
    {
        var byteIndex = 8 + (bitFromMsd8 / 8);
        var bitPos = bitFromMsd8 % 8;
        if (byteIndex >= payload.Length)
            return "n/a";

        bool set = (payload[byteIndex] & (1 << bitPos)) != 0;
        return (set ^ invert) ? "yes" : "no";
    }
}
