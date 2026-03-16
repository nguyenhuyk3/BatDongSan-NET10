using System.ComponentModel;
using System.Reflection;

namespace VTTGROUP.Domain.Helpers
{
    public static class EnumHelper
    {
        public static string GetDescription<TEnum>(this TEnum enumValue) where TEnum : Enum
        {
            var field = enumValue.GetType().GetField(enumValue.ToString());
            var attr = field?.GetCustomAttribute<DescriptionAttribute>();
            return attr?.Description ?? enumValue.ToString();
        }
    }
}
