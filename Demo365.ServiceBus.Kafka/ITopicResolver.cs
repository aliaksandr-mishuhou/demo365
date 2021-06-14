using System.Collections.Generic;

namespace Demo365.ServiceBus.Kafka
{
    public interface ITopicResolver
    {
        IEnumerable<string> Resolve<T>(T message);
    }
}
