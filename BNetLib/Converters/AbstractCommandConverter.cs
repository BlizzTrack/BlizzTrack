using BNetLib.Networking.Commands;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BNetLib.Converters
{
    public class AbstractCommandConverter : JsonConverter<AbstractCommand>
    {
        public override void WriteJson(JsonWriter writer, [AllowNull] AbstractCommand value, JsonSerializer serializer)
        {
            writer.WriteValue(value.ToString());
        }

        public override AbstractCommand ReadJson(JsonReader reader, Type objectType, [AllowNull] AbstractCommand existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            throw new Exception("Cannot read this value");
        }
    }
}
