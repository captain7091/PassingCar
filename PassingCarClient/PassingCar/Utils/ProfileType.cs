namespace PassingCar.Utils
{
    public enum ProfileType
    {
        Fisica,
        Juridica,
        JuridicaTransporte
    }
    public static class ProfileTypeExtension
    {
        public static string CustomToString(this ProfileType profileType)
        {
            switch (profileType)
            {
                case ProfileType.Fisica:
                    return $"Física";
                case ProfileType.Juridica:
                    return $"Juíridica";
                case ProfileType.JuridicaTransporte:
                    return $"Juíridica el transporte";
                default:
                    break;
            }
            return profileType.ToString();
        }
        public static ProfileType GetProfileType(this string value)
        {
            switch (value)
            {
                case "Física":
                    return ProfileType.Fisica;
                case "Juíridica":
                    return ProfileType.Juridica;
                case "Juíridica el transporte":
                    return ProfileType.JuridicaTransporte;
                default:
                    return ProfileType.Fisica;
            }
        }
    }
}
