using System.Text.RegularExpressions;

namespace Xplorer.Core.Utilities
{
    public class RegexUtil
    {
        public static readonly Regex MatchAny = new Regex("\\.*", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.IgnorePatternWhitespace | RegexOptions.Compiled);

        public static readonly Regex MatchConsecutiveSlashes = new Regex("[\\\\/]{1,}", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.IgnorePatternWhitespace | RegexOptions.Compiled);

        public static readonly Regex IsAbsoluteUrl = new Regex("^(http:|https:|ftp:)", RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);

        public static readonly Regex MatchIfStartsWithoutDot = new Regex("^[^\\.]*", RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);
    }
}
