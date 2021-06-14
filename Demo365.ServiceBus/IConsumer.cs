using System;
using System.Threading;
using System.Threading.Tasks;

namespace Demo365.ServiceBus
{
    /// <summary>
    /// queue listener
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IConsumer<T> : IDisposable
    {
        void Handle(T message);
        Task HandleAsync(T message, CancellationToken cancellationToken);
    }
}
