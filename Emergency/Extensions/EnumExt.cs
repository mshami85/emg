using System.ComponentModel;
using System.Reflection;

namespace Emergency
{
    public static class EnumExt
    {
        public static string ToDescription(this Enum value)
        {
            try
            {
                return value.GetType()
                            .GetMember(value.ToString())
                            .FirstOrDefault()?
                            .GetCustomAttribute<DescriptionAttribute>()?.Description ?? value.ToString();
            }
            catch
            {
                return value.ToString();
            }
            /*
            var descriptionAttribute = (DescriptionAttribute)value.GetType().GetField(value.ToString()).GetCustomAttribute(typeof(DescriptionAttribute));
            return descriptionAttribute.Description ?? value.ToString();
            */
        }
    }
}
