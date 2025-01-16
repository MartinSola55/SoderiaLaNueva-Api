namespace SoderiaLaNueva_Api.Models.Constants
{
    public static class CartStatuses
    {
        public const string Pending = "Pendiente";
        public const string Confirmed = "Confirmado";
        public const string Absent = "Ausente";
        public const string DidNotNeed = "No necesitó";
        public const string Holiday = "De vacaciones";

        public static bool Validate(string status)
        {
            return !string.IsNullOrEmpty(status) && typeof(CartStatuses).GetFields().Any(f => f.GetValue(null)?.ToString() == status);
        }

        public static List<string> GetCartStatuses()
        {
            var cartStatuses = new List<string>();

            foreach (var field in typeof(CartStatuses).GetFields())
            {
                var value = field.GetValue(null)?.ToString();
                if (!string.IsNullOrEmpty(value))
                    cartStatuses.Add(value);
            }

            return cartStatuses;
        }
    }
}
