namespace SoderiaLaNueva_Api.Models.Constants
{
    public static class ProductTypes
    {
        public const string B12L = "Bidón 12L";
        public const string B20L = "Bidón 20L";
        public const string Soda = "Soda";
        public const string Maquina = "Máquina frío/calor";

        public static List<string> GetProductTypes()
        {
            var statuses = new List<string>();

            foreach (var field in typeof(ProductTypes).GetFields())
            {
                var value = field.GetValue(null)?.ToString();
                if (!string.IsNullOrEmpty(value))
                    statuses.Add(value);
            }

            return statuses;
        }
    }
}
