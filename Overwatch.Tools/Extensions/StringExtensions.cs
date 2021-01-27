using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Overwatch.Tools.Extensions
{
    public static class StringExtensions
    {
        internal static string BattletagToUrlFriendlyString(this string battletag) => battletag.Replace('#', '-');
    }
}
