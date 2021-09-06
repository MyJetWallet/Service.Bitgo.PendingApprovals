using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Service.Bitgo.PendingApprovals.Utils
{
    public class OtpHiddenJsonConverter : JsonConverter
    {
        private readonly Type _type;
        private readonly string[] _pinValues = { "Otp" };

        public OtpHiddenJsonConverter(Type type)
        {
            _type = type;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            JToken t = JToken.FromObject(value);

            if (t.Type != JTokenType.Object)
            {
                t.WriteTo(writer);
            }
            else
            {
                JObject o = (JObject)t;
                IList<JProperty> propertyNames = o.Properties().Where(p => _pinValues.Contains(p.Name)).ToList();

                foreach (var property in propertyNames)
                {
                    string propertyValue = (string)property.Value;
                    if (propertyValue != null)
                    {
                        property.Value = "".PadRight(propertyValue.Length, '*');
                    }
                }

                o.WriteTo(writer);
            }
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue,
            JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override bool CanConvert(Type objectType)
        {
            return _type == objectType;
        }
    }
}