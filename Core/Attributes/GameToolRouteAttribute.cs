using Microsoft.AspNetCore.Mvc.Routing;
using System;
using System.Linq;

namespace Core.Attributes
{
    public class GameToolRouteAttribute : Attribute, IRouteTemplateProvider
    {
        public GameToolRouteAttribute(Type assembly, string controller = "")
        {
            var name = assembly.Assembly.GetName().Name;
            if (name != null)
                Template =
                    $"api/tools/{name.Split('.').First().ToLower()}{(!string.IsNullOrEmpty(controller) ? "/" + controller : "")}";
        }

        public string Template { get; }

        public int? Order => 2;

        public string Name => null;
    }
}
