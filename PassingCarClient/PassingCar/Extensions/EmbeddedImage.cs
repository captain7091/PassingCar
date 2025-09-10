

namespace PassingCar.Extensions
{
    public class EmbeddedImage : IMarkupExtension
    {
        public string fileName { get; set; }
        public object ProvideValue(IServiceProvider serviceProvider)
        {
            return string.IsNullOrEmpty(fileName) ? null : (object)$"{fileName}";
        }
    }
    public class EmbeddedSvg : IMarkupExtension
    {
        public string fileName { get; set; }
        public object ProvideValue(IServiceProvider serviceProvider)
        {
            return string.IsNullOrEmpty(fileName) ? null : (object)$"{fileName}";
        }
    }
}
