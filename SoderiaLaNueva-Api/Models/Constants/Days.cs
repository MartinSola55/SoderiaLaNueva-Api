namespace SoderiaLaNueva_Api.Models.Constants
{
    public static class Days
    {
        public const string Lunes = "Lunes";
        public const string Martes = "Martes";
        public const string Miercoles = "Miércoles";
        public const string Jueves = "Jueves";
        public const string Viernes = "Viernes";

        public static string GetDayByIndex(int? day)
        {
            if (!day.HasValue)
                return "Sin día de reparto asignado";

            return day switch
            {
                1 => Lunes,
                2 => Martes,
                3 => Miercoles,
                4 => Jueves,
                5 => Viernes,
                _ => "Sin día de reparto asignado"
            };
        }
    }
}
