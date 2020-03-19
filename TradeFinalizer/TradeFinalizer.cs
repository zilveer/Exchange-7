using Infrastructure;
using OrderMatcher;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace TradeFinalizer
{
    class TradeFinalizer
    {
        private QueueConsumer _queueConsumer;
        private Task _inboundQueueProcessor;
        private Task _inboundChannelProcessor;
        private Channel<Message> _inboundChannel;
        private ChannelWriter<Message> _inboundChannelWritter;
        private ChannelReader<Message> _inboundChannelReader;
        public void Start(QueueConfig queueConfig)
        {
            _queueConsumer = new QueueConsumer(queueConfig.ExchangeName, queueConfig.ExchangeType, queueConfig.QueueName, queueConfig.RoutingKey, queueConfig.HostName, queueConfig.Vhost, queueConfig.User, queueConfig.Password);
            _inboundQueueProcessor = Task.Factory.StartNew(QueueConsumer);

            _inboundChannel = Channel.CreateBounded<Message>(new BoundedChannelOptions(100000) { FullMode = BoundedChannelFullMode.Wait, SingleReader = true, SingleWriter = true, AllowSynchronousContinuations = false });
            _inboundChannelReader = _inboundChannel.Reader;
            _inboundChannelWritter = _inboundChannel.Writer;

            _inboundChannelProcessor = Task.Factory.StartNew(async () => await InboundMessageProcessor());
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
                case MessageType.Cancel:
                    obj = CancelledOrderSerializer.Deserialize(body);
                    break;
                case MessageType.Fill:
                    obj = FillSerializer.Deserialize(body);
                    break;
                case MessageType.OrderTrigger:
                    obj = OrderTriggerSerializer.Deserialize(body);
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
                                case MessageType.Cancel:
                                    Console.WriteLine($"Cancel : {((CancelledOrder)message.Object).OrderId}");
                                    break;
                                case MessageType.Fill:
                                    Console.WriteLine($"Fill : {((Fill)message.Object).MakerOrderId} {((Fill)message.Object).TakerOrderId} {((Fill)message.Object).MatchRate} {((Fill)message.Object).MatchQuantity}");
                                    break;
                                case MessageType.OrderTrigger:
                                    Console.WriteLine($"Trigger : {((OrderTrigger)message.Object).OrderId }");
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
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                //TODO
                throw;
            }
        }


        private void CancelOrder()
        {

        }

        private void Fill()
        {

        }

        private void Trigger()
        {

        }

        private void Accept()
        {

        }

        public void Stop()
        {
            _queueConsumer.Stop();
            _inboundChannelWritter.Complete();
            _inboundQueueProcessor.Wait();
            _inboundChannelProcessor.Wait();
        }
    }
}
