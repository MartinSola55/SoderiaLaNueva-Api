namespace SoderiaLaNueva_Api.Models.Constants
{
    public static class TaxCondition
    {
        public const string RI = "Responsable Inscripto";
        public const string MO = "Monotributo";
        public const string EX = "Exento";
        public const string CF = "Consumidor Final";

        public static bool Validate(string condition)
        {
            return !string.IsNullOrEmpty(condition) && typeof(TaxCondition).GetFields().Any(f => f.GetValue(null)?.ToString() == condition);
        }

        public static List<string> GetAll()
        {
            var conditions = new List<string>();

            foreach (var field in typeof(TaxCondition).GetFields())
            {
                var value = field.GetValue(null)?.ToString();
                if (!string.IsNullOrEmpty(value))
                    conditions.Add(value);
            }

            return conditions;
        }
    }
}
