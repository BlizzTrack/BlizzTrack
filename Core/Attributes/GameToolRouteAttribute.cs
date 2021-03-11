using Microsoft.AspNetCore.Mvc.Routing;
using System;
using System.Linq;

namespace Core.Attributes
{
    public class GameToolRouteAttribute : Attribute, IRouteTemplateProvider
    {
        public GameToolRouteAttribute(Type assembly, string controller = "")
        {
            Template = $"api/tools/{assembly.Assembly.GetName().Name.Split('.').First().ToLower()}{(!string.IsNullOrEmpty(controller) ? "/" + controller : "")}";
        }

        public string Template { get; set; }

        public int? Order => 2;

        public string Name { get; set; }
    }
}
