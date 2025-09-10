using PassingCar.Models;

namespace PassingCar.Extensions
{
    public static class EnumsExtensions
    {
        public static string Espana(this AdsState state)
        {
            switch (state)
            {
                case AdsState.Pending:
                    return "Disponible";
                case AdsState.Posted:
                    return "Disponible";
                case AdsState.PaymentPending:
                    return "Se espera el pago";
                case AdsState.AcceptedForTransit:
                    return "Todo listo para el transporte";
                case AdsState.InTransit:
                    return "En tránsito";
                case AdsState.Delivered:
                    return "Entregado";
                case AdsState.Closed:
                    return "Cerrado";
                case AdsState.Disabled:
                    return "Discapacitado";
                case AdsState.Blocked:
                    return "Obstruido";
                default:
                    return "Sin detalles";
            }
        }
        public static string ReplaceEspana(this string enumValue)
        {
            if (!string.IsNullOrEmpty(enumValue))
            {
                switch (enumValue)
                {
                    case "Disponible":
                        return "Posted";
                    case "Se espera el pago":
                        return "PaymentPending";
                    case "Todo listo para el transporte":
                        return "AcceptedForTransit";
                    case "En tránsito":
                        return "InTransit";
                    case "Entregado":
                        return "Delivered";
                    case "Cerrado":
                        return "Closed";
                    case "Discapacitado":
                        return "Disabled";
                    case "Obstruido":
                        return "Blocked";
                    default:
                        return enumValue;
                }
            }
            else
            {
                return enumValue;
            }
        }
    }
}
