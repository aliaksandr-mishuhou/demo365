using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Demo365.ServiceBus
{
    /// <summary>
    /// message sender
    /// </summary>
    public interface IPublisher : IDisposable
    {
        bool Send<T>(T message, string groupKey = null);

        Task<bool> SendAsync<T>(T message, string groupKey = null);

        Task<bool> SendRawAsync(string json, string topic = null, string groupKey = null);

        Task SendBatchAsync<T>(IEnumerable<T> messages);

        Task SendBatchRawAsync(IEnumerable<string> messages);
    }
}
