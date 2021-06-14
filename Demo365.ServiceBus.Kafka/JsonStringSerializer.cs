using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Demo365.ServiceBus.Kafka
{
    public class JsonStringSerializer : IStringSerializer
    {
        private readonly bool _ignoreDefaults;
        private readonly IContractResolver _customContractResolver;

        public JsonStringSerializer(bool ignoreDefaults = true, IContractResolver customContractResolver = null)
        {
            _ignoreDefaults = ignoreDefaults;
            _customContractResolver = customContractResolver;
        }

        public string Serialize<T>(T obj)
        {
            var settings = new JsonSerializerSettings
            {
                DefaultValueHandling = _ignoreDefaults ? DefaultValueHandling.Ignore : DefaultValueHandling.Include,
            };

            if (_customContractResolver != null)
            {
                settings.ContractResolver = _customContractResolver;
            }

            return JsonConvert.SerializeObject(obj, settings);
        }

        public T Deserialize<T>(string data)
        {
            if (_customContractResolver != null)
            {
                var settings = new JsonSerializerSettings
                {
                    ContractResolver = _customContractResolver,
                };

                return JsonConvert.DeserializeObject<T>(data, settings);
            }

            return JsonConvert.DeserializeObject<T>(data);
        }
    }
}
