using System.Drawing;
using System.Globalization;

namespace controller;

internal static class StimInputValidation
{
    public static bool TryParseInt(
        TextBox input,
        string fieldName,
        int min,
        int max,
        out int value,
        Action<TextBox, string> onError)
    {
        if (!int.TryParse(input.Text.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out value))
        {
            onError(input, $"{fieldName}: enter a whole number.");
            return false;
        }

        if (value < min || value > max)
        {
            onError(input, $"{fieldName}: value must be between {min} and {max}.");
            return false;
        }

        MarkValid(input);
        return true;
    }

    public static bool TryParseDecimal(
        TextBox input,
        string fieldName,
        decimal min,
        decimal max,
        out decimal value,
        Action<TextBox, string> onError)
    {
        if (!decimal.TryParse(input.Text.Trim(), NumberStyles.Number, CultureInfo.InvariantCulture, out value))
        {
            onError(input, $"{fieldName}: enter a number.");
            return false;
        }

        if (value < min || value > max)
        {
            onError(input, $"{fieldName}: value must be between {min} and {max}.");
            return false;
        }

        MarkValid(input);
        return true;
    }

    public static void MarkInvalid(TextBox input) =>
        input.BackColor = Color.MistyRose;

    public static void MarkValid(TextBox input) =>
        input.BackColor = SystemColors.Window;
}
