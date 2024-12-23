using System.Globalization;

namespace SoderiaLaNueva_Api.Helpers
{
    public class Formatting
    {
        private static readonly CultureInfo _culture = new("es-AR");

        public static string FormatDecimal(decimal value)
        {
            return value.ToString("N2", _culture);
        }

        public static string FormatNumber(int value)
        {
            return value.ToString("N0", _culture);
        }

        public static string FormatCurrency(decimal value)
        {
            return $"${value.ToString("N2", _culture)}";
        }

        public static string FormatCurrency(int value)
        {
            return $"${value.ToString("N2", _culture)}";
        }
    }
}
