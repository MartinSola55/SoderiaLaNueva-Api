namespace SoderiaLaNueva_Api.Models.Constants
{
    public class CartsTransfersType
    {
        public const string Cart = "Bajada";
        public const string Subscription = "Abono";

        public static List<string> GetCartsTransfersTypes()
        {
            var types = new List<string>();

            foreach (var field in typeof(CartsTransfersType).GetFields())
            {
                var value = field.GetValue(null)?.ToString();
                if (!string.IsNullOrEmpty(value))
                    types.Add(value);
            }

            return types;
        }
    }
}
