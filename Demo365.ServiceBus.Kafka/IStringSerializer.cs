namespace Demo365.ServiceBus.Kafka
{
    public interface IStringSerializer
    {
        string Serialize<T>(T obj);

        T Deserialize<T>(string data);
    }
}
