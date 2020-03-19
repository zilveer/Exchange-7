using RabbitMQ.Client;

namespace Infrastructure
{
    public class QueueProducer : IQueueProducer
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

        public QueueProducer(string exchangeName, string exchangeType, string queueName, string routingKey, string hostName, string vhost, string user, string pass)
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

        public void Produce(byte[] message, string routingKey = "")
        {
            Model.BasicPublish(_exchangeName, routingKey, null, message);
        }

        public void Flush()
        {
            Model.ConfirmSelect();
            Model.WaitForConfirms();
        }

        public void Close()
        {
            if (Model.IsClosed == false)
                Model.Close();
            _connection.Close();
        }
    }
}
