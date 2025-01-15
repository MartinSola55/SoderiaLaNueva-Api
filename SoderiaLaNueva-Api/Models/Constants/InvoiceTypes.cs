namespace SoderiaLaNueva_Api.Models.Constants
{
    public static class InvoiceTypes
    {
        public const string A = "A";
        public const string B = "B";

        public static bool Validate(string type)
        {
            return !string.IsNullOrEmpty(type) && typeof(InvoiceTypes).GetFields().Any(f => f.GetValue(null)?.ToString() == type);
        }

        public static List<string> GetAll()
        {
            var types = new List<string>();

            foreach (var field in typeof(InvoiceTypes).GetFields())
            {
                var value = field.GetValue(null)?.ToString();
                if (!string.IsNullOrEmpty(value))
                    types.Add(value);
            }

            return types;
        }
    }
}
