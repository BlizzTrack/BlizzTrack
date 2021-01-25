using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace BlizzTrack.Constraint
{
    public class DescribeEnumMemberValues : ISchemaFilter
    {
        public void Apply(OpenApiSchema schema, SchemaFilterContext context)
        {
            if (context.Type.IsEnum)
            {
                schema.Enum.Clear();

                //Retrieve each of the values decorated with an EnumMember attribute
                foreach (var member in context.Type.GetMembers())
                {
                    var memberAttr = member.GetCustomAttributes(typeof(EnumMemberAttribute), false).FirstOrDefault();
                    if (memberAttr != null)
                    {
                        var attr = (EnumMemberAttribute)memberAttr;
                        schema.Enum.Add(new OpenApiString(attr.Value));
                    }
                }
            }
        }
    }
}
