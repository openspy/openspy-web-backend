using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoreWeb.Filters
{
    class DateObject
    {
        public int month;
        public int day;
        public int year;
    };
    public class JsonDateConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(DateTime?) || objectType == typeof(DateTime);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if(reader.TokenType != JsonToken.StartObject) return null;
            var jobject = JObject.Load(reader);
            DateObject date = jobject.ToObject<DateObject>();
            return new DateTime(date.year, date.month, date.day);

        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            DateTime date = (DateTime)value;
            DateObject _do = new DateObject();
            _do.year = date.Year;
            _do.month = date.Month;
            _do.day = date.Day;
            serializer.Serialize(writer, _do);
        }
    }
}