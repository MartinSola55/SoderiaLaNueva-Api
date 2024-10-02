namespace SoderiaLaNueva_Api.Models.Constants
{
    public static class CartStatus
    {
        public const string Pending = "Pendiente";
        public const string Confirmed = "Confirmado";
        public const string Absent = "Ausente";
        public const string DidNotNeed = "No necesitó";
        public const string Holiday = "De vacaciones";

        public static bool Validate(string status)
        {
            return !string.IsNullOrEmpty(status) && typeof(CartStatus).GetFields().Any(f => f.GetValue(null)?.ToString() == status);
        }
    }
}
