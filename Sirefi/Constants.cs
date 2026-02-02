namespace Sirefi;

public static class Constants
{
    public static class Roles
    {
        public const string Administrador = "administrador";
        public const string Reportante = "reportante";
        public const string Tecnico = "tecnico";
    }

    public static class Estados
    {
        public const string Recibido = "Recibido";
        public const string EnProceso = "En proceso";
        public const string Solucionado = "Solucionado";
        public const string Cancelado = "Cancelado";
    }

    public static class Prioridades
    {
        public const string Alta = "Alta";
        public const string Media = "Media";
        public const string Baja = "Baja";
    }
}
