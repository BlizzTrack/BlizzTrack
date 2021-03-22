using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BlizzTrack.Constraint
{
    public class EnumConstraint : IRouteConstraint
    {
        private static readonly ConcurrentDictionary<string, string[]> Cache = new();
        private readonly string[] _validOptions;
        /// <summary>
        /// Create new <see cref="EnumConstraint"/>
        /// </summary>
        /// <param name="enumType"></param>
        public EnumConstraint(string enumType)
        {
            _validOptions = Cache.GetOrAdd(enumType, key =>
            {
                var type = Type.GetType(key);
                return type != null ? Enum.GetNames(type) : Array.Empty<string>();
            });
        }

        public bool Match(HttpContext httpContext, IRouter route, string routeKey, RouteValueDictionary values, RouteDirection routeDirection)
        {
            var candidate = values[routeKey]?.ToString();

            if (values.TryGetValue(candidate ?? string.Empty, out var value) && value != null)
            {
                return _validOptions.Contains(value.ToString(), StringComparer.OrdinalIgnoreCase);
            }
            return false;
        }
    }
}
