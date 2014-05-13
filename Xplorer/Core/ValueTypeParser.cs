using System;

namespace WebSite.Core
{
    public static class ValueTypeParser
    {
        public static DateTime GetDateTime(string value, string format, DateTime defaultValue)
        {
            DateTime returnValue = defaultValue;
            if (DateTime.TryParseExact(value, format, null, System.Globalization.DateTimeStyles.None, out returnValue) == false)
                return defaultValue;
            return returnValue;
        }

        public static bool TryParseDateTime(string value, string format, out DateTime defaultValue)
        {
            return DateTime.TryParseExact(value, format, null, System.Globalization.DateTimeStyles.None, out defaultValue);
        }

        public static T Parse<T>(string arg, T defaultValue)
        {
            try
            {
                if (string.IsNullOrEmpty(arg))
                    return defaultValue;
                return (T)Convert.ChangeType(arg, typeof(T));
            }
            catch (Exception)
            {
                return defaultValue;
            }
        }
    }
}