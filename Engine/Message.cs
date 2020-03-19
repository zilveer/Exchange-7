using OrderMatcher;

namespace Engine
{
    public class Message
    {
        private readonly MessageType _type;
        private readonly byte[] _body;
        private readonly object _object;

        public Message(MessageType type, byte[] body, object obj)
        {
            _type = type;
            _body = body;
            _object = obj;
        }

        public MessageType MessageType => _type;
        public byte[] Bytes => _body;
        public object Object => _object;
    }
}
