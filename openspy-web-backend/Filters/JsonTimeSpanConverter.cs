using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoreWeb.Filters
{
    public class JsonTimeSpanConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(TimeSpan);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.Value == null) { return null; }
            
            System.UInt32 v;
            if (!System.UInt32.TryParse(reader.Value.ToString(), out v))
            {
                return null;
            }
            System.TimeSpan tsTimeSpan = TimeSpan.FromSeconds(v);
            return tsTimeSpan;

        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            TimeSpan elapsedTime = (TimeSpan)value;
            writer.WriteRawValue(((int)elapsedTime.TotalSeconds).ToString());
        }
    }
}