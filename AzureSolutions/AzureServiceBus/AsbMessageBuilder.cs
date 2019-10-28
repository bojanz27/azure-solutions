using System;
using System.Collections.Generic;
using Microsoft.Azure.ServiceBus;

namespace AzureSolutions.Messaging.AzureServiceBus
{
    public class AsbMessageBuilder
    {
        private readonly Message _message;

        private AsbMessageBuilder() => _message = new Message();

        public static AsbMessageBuilder NewMessage() => new AsbMessageBuilder();

        public AsbMessageBuilder WithMessageId(string id)
        {
            _message.MessageId = id;
            return this;
        }

        public AsbMessageBuilder WithBody(byte[] body)
        {
            _message.Body = body;
            return this;
        }

        public AsbMessageBuilder WithParameter(string paramKey, object paramVal)
        {
            _message.UserProperties.Add(new KeyValuePair<string, object>(paramKey, paramVal));
            return this;
        }

        public AsbMessageBuilder WithContentType(Type type)
        {
            _message.ContentType = type.ToString();
            return this;
        }

        public static implicit operator Message(AsbMessageBuilder self) => self?._message;
    }
}
