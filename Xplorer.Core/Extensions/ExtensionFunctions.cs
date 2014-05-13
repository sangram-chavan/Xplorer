using System;
using System.Collections.Generic;
using System.Text;
using Xplorer.Core.Utilities;

namespace Xplorer.Core.Extensions
{
    public static class ExtensionFunctions
    {
        /// <summary>
        /// Encodes to base64.
        /// </summary>
        /// <param name="target">The target.</param>
        /// <returns></returns>
        public static string EncodeToBase64(this string target)
        {
            if (target == null) return string.Empty;
            return Convert.ToBase64String(Encoding.Default.GetBytes(target));
        }

        /// <summary>
        /// Decodes from base64.
        /// </summary>
        /// <param name="target">The target.</param>
        /// <returns></returns>
        public static string DecodeFromBase64(this string target)
        {
            if (target == null) return string.Empty;
            try
            {
                return Encoding.Default.GetString(Convert.FromBase64String(target));
            }
            catch
            {
                return target;
            }
        }
    
        public static string FormatWith(this string s, params object[] args)
        {
            return string.Format(s, args);
        }
    
        public static string ReplaceConsecutiveSlashes(this string s,string replacement)
        {
            return RegexUtil.MatchConsecutiveSlashes.Replace(s, replacement);
        }

        public static string ReplaceConsecutiveSlashes(this string s, string replacement,int startat)
        {
            return RegexUtil.MatchConsecutiveSlashes.Replace(s, replacement,255,startat);
        }

        public static string RemoveSlashes(this string s)
        {
            return s.Trim(new[] { '\\', '/' });
        }

        public static string RemoveStartSlashes(this string s)
        {
            return s.TrimStart(new[] { '\\', '/' });
        }

        public static string RemoveEndSlashes(this string s)
        {
            return s.TrimEnd(new[] { '\\', '/' });
        }

        public static void ForEach<T>(this IEnumerable<T> enumerable, Action<T> action)
        {
            var list = enumerable as List<T>;
            if (list != null)
                list.ForEach(action);
            else
            {
                foreach (T item in enumerable)
                    action(item);
            }
        }

        public static bool Is<T>(this Type type)
        {
            return type != null && typeof(T).IsAssignableFrom(type);
        }

    }
}

