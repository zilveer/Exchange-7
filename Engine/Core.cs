using Infrastructure;
using OrderMatcher;
using System;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Engine
{
    public class Core
    {
        private readonly ITimeProvider _timeProvider;
        private Channel<Message> _inboundChannel;
        private Channel<byte[]> _outboundChannel;
        private Channel<byte[]> _outboundQueueChannel;
        private Channel<byte[]> _outboundFileChannel;
        private ChannelWriter<Message> _inboundChannelWritter;
        private ChannelReader<Message> _inboundChannelReader;
        private ChannelWriter<byte[]> _outboundChannelWritter;
        private ChannelReader<byte[]> _outboundChannelReader;
        private ChannelWriter<byte[]> _outboundQueueChannelWritter;
        private ChannelReader<byte[]> _outboundQueueChannelReader;
        private ChannelWriter<byte[]> _outboundFileChannelWritter;
        private ChannelReader<byte[]> _outboundFileChannelReader;

        private MatchingEngine _matchingEngine;
        private TradeLogger _tradeLogger;
        private QueueConsumer _queueConsumer;
        private QueueProducer _queueProducer;
        private Task _inboundQueueProcessor;
        private Task _inboundChannelProcessor;
        private Task _outboundChannelProcessor;
        private Task _outboundQueueChannelProcessor;
        private Task _outboundFileChannelProcessor;

        public Core()
        {
            _timeProvider = new TimeProvider();
        }

        public void Start(QueueConfig consumerQueuConfig, QueueConfig producerQueueConfig, string filePath, ITimeProvider timeProvider, int quoteCurrencyDecimalPlances, int stepSize)
        {
            _outboundChannel = Channel.CreateBounded<byte[]>(new BoundedChannelOptions(100000) { FullMode = BoundedChannelFullMode.Wait, SingleReader = true, SingleWriter = true, AllowSynchronousContinuations = false });
            _outboundChannelReader = _outboundChannel.Reader;
            _outboundChannelWritter = _outboundChannel.Writer;

            var tradeListner = new TradeListener(_outboundChannelWritter, timeProvider);
            _matchingEngine = new MatchingEngine(quoteCurrencyDecimalPlances, stepSize, tradeListner, new TimeProvider());

            _inboundChannel = Channel.CreateBounded<Message>(new BoundedChannelOptions(100000) { FullMode = BoundedChannelFullMode.Wait, SingleReader = true, SingleWriter = true, AllowSynchronousContinuations = false });
            _inboundChannelReader = _inboundChannel.Reader;
            _inboundChannelWritter = _inboundChannel.Writer;

            _outboundQueueChannel = Channel.CreateBounded<byte[]>(new BoundedChannelOptions(100000) { FullMode = BoundedChannelFullMode.Wait, SingleReader = true, SingleWriter = true, AllowSynchronousContinuations = false });
            _outboundQueueChannelReader = _outboundQueueChannel.Reader;
            _outboundQueueChannelWritter = _outboundQueueChannel.Writer;

            _outboundFileChannel = Channel.CreateBounded<byte[]>(new BoundedChannelOptions(100000) { FullMode = BoundedChannelFullMode.Wait, SingleReader = true, SingleWriter = true, AllowSynchronousContinuations = false });
            _outboundFileChannelReader = _outboundFileChannel.Reader;
            _outboundFileChannelWritter = _outboundFileChannel.Writer;

            _tradeLogger = new TradeLogger(filePath);
            _queueProducer = new QueueProducer(producerQueueConfig.ExchangeName, producerQueueConfig.ExchangeType, producerQueueConfig.QueueName, producerQueueConfig.RoutingKey, producerQueueConfig.HostName, producerQueueConfig.Vhost, producerQueueConfig.User, producerQueueConfig.Password);
            _queueConsumer = new QueueConsumer(consumerQueuConfig.ExchangeName, consumerQueuConfig.ExchangeType, consumerQueuConfig.QueueName, consumerQueuConfig.RoutingKey, consumerQueuConfig.HostName, consumerQueuConfig.Vhost, consumerQueuConfig.User, consumerQueuConfig.Password);

            _inboundQueueProcessor = Task.Factory.StartNew(QueueConsumer);
            _inboundChannelProcessor = Task.Factory.StartNew(async () => await InboundMessageProcessor());
            _outboundChannelProcessor = Task.Factory.StartNew(async () => await OutboundMessageProcessor());
            _outboundQueueChannelProcessor = Task.Factory.StartNew(async () => await OutboundQueueMessageProcessor());
            _outboundFileChannelProcessor = Task.Factory.StartNew(async () => await OutboundFileMessageProcessor());
        }

        public void Stop()
        {
            _queueConsumer.Stop();
            _inboundChannelWritter.Complete();
            _inboundQueueProcessor.Wait();
            _inboundChannelProcessor.Wait();
            _outboundChannelProcessor.Wait();
            _outboundQueueChannelProcessor.Wait();
            _outboundFileChannelProcessor.Wait();
        }

        private void QueueConsumer()
        {
            try
            {
                _queueConsumer.Consume(OnMessage);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                throw;
            }
        }

        private void OnMessage(byte[] body)
        {
            var type = Serializer.GetMessageType(body);
            object obj = null;
            switch (type)
            {
                case MessageType.NewOrderRequest:
                    obj = OrderSerializer.Deserialize(body);
                    break;
                case MessageType.CancelRequest:
                    obj = CancelRequestSerializer.Deserialize(body);
                    break;
                case MessageType.BookRequest:
                    obj = BookRequestSerializer.Deserialize(body);
                    break;
                default:
                    //TODO
                    break;
            }
            var message = new Message(type.Value, body, obj);
            _inboundChannelWritter.TryWrite(message);
        }

        private async Task InboundMessageProcessor()
        {
            try
            {
                while (await _inboundChannelReader.WaitToReadAsync())
                {
                    while (_inboundChannelReader.TryRead(out Message message))
                    {
                        try
                        {
                            switch (message.MessageType)
                            {
                                case MessageType.NewOrderRequest:
                                    _matchingEngine.AddOrder((OrderWrapper)message.Object);
                                    break;
                                case MessageType.CancelRequest:
                                    _matchingEngine.CancelOrder(((CancelRequest)message.Object).OrderId);
                                    break;
                                case MessageType.BookRequest:
                                    var bytes = BookSerializer.Serialize(_matchingEngine.Book, ((BookRequest)message.Object).LevelCount, _matchingEngine.MarketPrice, _timeProvider.GetSecondsFromEpoch());
                                    _outboundChannelWritter.TryWrite(bytes);
                                    //TODO
                                    break;
                                default:
                                    //TODO
                                    break;
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex);
                            //TODO
                            throw;
                        }
                    }
                }
                _outboundChannelWritter.Complete();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                //TODO
                throw;
            }
        }

        private async Task OutboundMessageProcessor()
        {
            try
            {
                while (await _outboundChannelReader.WaitToReadAsync())
                {
                    while (_outboundChannelReader.TryRead(out byte[] data))
                    {
                        try
                        {
                            _outboundQueueChannelWritter.TryWrite(data);
                            _outboundFileChannelWritter.TryWrite(data);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex);
                            //TODO
                            throw;
                        }
                    }
                }
                _outboundQueueChannelWritter.Complete();
                _outboundFileChannelWritter.Complete();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                //TODO
                throw;
            }
        }

        private async Task OutboundQueueMessageProcessor()
        {
            try
            {
                while (await _outboundQueueChannelReader.WaitToReadAsync())
                {
                    while (_outboundQueueChannelReader.TryRead(out byte[] data))
                    {
                        try
                        {
                            _queueProducer.Produce(data);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex);
                            //TODO
                            throw;
                        }
                    }
                }
                _queueProducer.Close();
                _queueProducer.Flush();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                //TODO
                throw;
            }
        }

        private async Task OutboundFileMessageProcessor()
        {
            try
            {
                while (await _outboundFileChannelReader.WaitToReadAsync())
                {
                    while (_outboundFileChannelReader.TryRead(out byte[] data))
                    {
                        try
                        {
                            _tradeLogger.Write(data);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex);
                            //TODO
                            throw;
                        }
                    }
                }
                _tradeLogger.Complete();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                //TODO
                throw;
            }
        }
    }
}
