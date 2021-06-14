namespace Demo365.ServiceBus.Kafka
{
    public class KafkaProducerConfig
    {
        public ITopicResolver TopicResolver { get; set; }
        public string ConnectionString { get; set; }
    }
}
