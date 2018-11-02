using System;

namespace ReportService.Extensions
{
    public class EnumExtensions
    {
        public static class EnumHelper
        {
            public static T GetEnumValue<T>(string str) where T : struct
            {
                return Enum.TryParse<T>(str, true, out var val) ? val : default(T);
            }

            public static T GetEnumValue<T>(int intValue) where T : struct, IConvertible
            {
                return (T)Enum.ToObject(typeof(T), intValue);
            }
        }
    }
}
