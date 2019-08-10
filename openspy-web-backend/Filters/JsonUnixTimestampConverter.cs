using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoreWeb.Filters
{
    public class JsonDateTimeConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(DateTime);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.Value == null) { return null; }
            System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            System.UInt32 v;
            if (!System.UInt32.TryParse(reader.Value.ToString(), out v))
            {
                return null;
            }
            dtDateTime = dtDateTime.AddSeconds(v).ToLocalTime();
            return dtDateTime;

        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var Epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            TimeSpan elapsedTime = (DateTime)value - Epoch;
            writer.WriteRawValue(((int)elapsedTime.TotalSeconds).ToString());
        }
    }
}