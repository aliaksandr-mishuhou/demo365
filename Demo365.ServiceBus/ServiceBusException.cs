using System;
using System.Runtime.Serialization;

namespace Demo365.ServiceBus
{
    public class ServiceBusException : Exception
    {
        public ServiceBusException()
        {
        }

        public ServiceBusException(string message) : base(message)
        {
        }

        public ServiceBusException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected ServiceBusException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
