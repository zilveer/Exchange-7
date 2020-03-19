using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;

namespace Infrastructure
{
    public class QueueConsumer
    {
        private readonly object _lockObject;
        private readonly string _exchangeName;
        private readonly string _exchangeType;
        public readonly string _queueName;
        public readonly string _routingKey;
        private readonly string _user;
        private readonly string _pass;
        private readonly string _vhost;
        private readonly string _hostname;
        private string _consumerTag;
        private IModel _model;
        private IConnection _connection;

        private IModel Model
        {
            get
            {
                if (_model == null && _connection == null)
                {
                    lock (_lockObject)
                    {
                        if (_model == null && _connection == null)
                        {
                            ConnectionFactory factory = new ConnectionFactory();

                            factory.UserName = _user;
                            factory.Password = _pass;
                            factory.VirtualHost = _vhost;
                            factory.HostName = _hostname;

                            _connection = factory.CreateConnection();
                            _model = _connection.CreateModel();

                            _model.ExchangeDeclare(_exchangeName, _exchangeType, true, false);
                            _model.QueueDeclare(_queueName, true, false, false, null);
                            _model.QueueBind(_queueName, _exchangeName, _routingKey, null);

                        }
                    }
                }
                return _model;
            }
        }

        public QueueConsumer(string exchangeName, string exchangeType, string queueName, string routingKey, string hostName, string vhost, string user, string pass)
        {
            _lockObject = new object();
            _exchangeName = exchangeName;
            _exchangeType = exchangeType;
            _queueName = queueName;
            _routingKey = routingKey;
            _hostname = hostName;
            _vhost = vhost;
            _user = user;
            _pass = pass;
        }

        public void Consume(Action<byte[]> onMessage)
        {
            var consumer = new Consumer(Model, onMessage);
            _consumerTag = Model.BasicConsume(_queueName, false, consumer);
        }

        public void Stop()
        {
            if (!Model.IsClosed)
            {
                Model.BasicCancel(_consumerTag);
                _connection.Close();
                _consumerTag = null;
            }
        }
    }

    public class Consumer : IBasicConsumer
    {
        private readonly IModel _model;
        private readonly Action<byte[]> _onMessage;
        public Consumer(IModel model, Action<byte[]> onMessage)
        {
            _model = model;
            _onMessage = onMessage;
        }

        public IModel Model => _model;

        public event EventHandler<ConsumerEventArgs> ConsumerCancelled;

        public void HandleBasicCancel(string consumerTag)
        {
            try
            {
                ConsumerCancelled?.Invoke(this, new ConsumerEventArgs(consumerTag));
            }
            catch (Exception) { }
        }

        public void HandleBasicCancelOk(string consumerTag)
        {
        }

        public void HandleBasicConsumeOk(string consumerTag)
        {
        }

        public void HandleBasicDeliver(string consumerTag, ulong deliveryTag, bool redelivered, string exchange, string routingKey, IBasicProperties properties, byte[] body)
        {
            _onMessage(body);
            Model.BasicAck(deliveryTag, false);
        }

        public void HandleModelShutdown(object model, ShutdownEventArgs reason)
        {
        }
    }
}
