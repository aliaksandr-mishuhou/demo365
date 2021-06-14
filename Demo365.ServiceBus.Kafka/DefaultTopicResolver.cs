using System.Collections.Generic;

namespace Demo365.ServiceBus.Kafka
{
    public class DefaultTopicResolver : ITopicResolver
    {
        private readonly string _topic;

        public DefaultTopicResolver(string topic)
        {
            _topic = topic;
        }

        public IEnumerable<string> Resolve<T>(T message)
        {
            return new[] { _topic };
        }
    }
}
