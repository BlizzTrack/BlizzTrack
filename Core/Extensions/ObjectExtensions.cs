using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Core.Extensions
{ 
    public static class ObjectExtensions
    {
        public static T ToObject<T>(this IDictionary<string, object> source, BindingFlags bindingAttr = BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance) where T : class, new()
        {
            T someObject = new T();
            Assert.IsTrue(someObject != null, $"{nameof(someObject)} was null");

            var someObjectType = someObject.GetType();

            foreach (var (key, value) in source)
            {
                someObjectType
                         .GetProperty(key, bindingAttr)
                         ?.SetValue(someObject, value, null);
            }

            return someObject;
        }

        public static IDictionary<string, object> AsDictionary(this object source, BindingFlags bindingAttr = BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance)
        {
            return source.GetType().GetProperties(bindingAttr).ToDictionary
            (
                propInfo => propInfo.Name,
                propInfo => propInfo.GetValue(source, null)
            );

        }
    }
}
