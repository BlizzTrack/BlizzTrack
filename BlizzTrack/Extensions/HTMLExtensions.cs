using Microsoft.AspNetCore.Mvc.Rendering;

namespace BlizzTrack.Extensions
{
    public static class HTMLExtensions
    {
        public static bool IsDebug(this IHtmlHelper htmlHelper)
        {
#if DEBUG
            return true;
#else
      return false;
#endif
        }
    }
}
