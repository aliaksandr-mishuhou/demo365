namespace Demo365.ServiceBus.Kafka
{
    public class KafkaConsumerConfig
    {
        public string Topic { get; set; }
        public string ConnectionString { get; set; }
        public string GroupId { get; set; }
        public string AutoOffsetReset { get; set; }
        public override string ToString()
        {
            return $"KafkaConsumerConfig:{nameof(Topic)}:{Topic}-{nameof(ConnectionString)}:{ConnectionString}-{nameof(GroupId)}:{GroupId}";
        }
    }
}
