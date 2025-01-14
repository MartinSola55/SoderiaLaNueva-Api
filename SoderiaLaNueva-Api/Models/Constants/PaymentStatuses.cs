namespace SoderiaLaNueva_Api.Models.Constants
{
    public static class PaymentStatuses
    {
        public const string Pending = "Pendiente";
        public const string Completed = "Realizado";

        public static List<string> GetPaymentStatuses()
        {
            var statuses = new List<string>();

            foreach (var field in typeof(PaymentStatuses).GetFields())
            {
                var value = field.GetValue(null)?.ToString();
                if (!string.IsNullOrEmpty(value))
                    statuses.Add(value);
            }

            return statuses;
        }
    }
}
