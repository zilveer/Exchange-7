using OrderMatcher;
using System.Threading.Channels;

namespace Engine
{
    class TradeListener : ITradeListener
    {
        private readonly ITimeProvider _timeProvider;
        private readonly ChannelWriter<byte[]> _channelWriter;
        public TradeListener(ChannelWriter<byte[]> channelWriter, ITimeProvider timeProvider)
        {
            _channelWriter = channelWriter;
            _timeProvider = timeProvider;
        }

        public void OnCancel(OrderId orderId, Quantity remainingQuantity, Quantity remainingOrderAmount, CancelReason cancelReason)
        {
            var bytes = CancelledOrderSerializer.Serialize(orderId, remainingQuantity, remainingOrderAmount, cancelReason, _timeProvider.GetSecondsFromEpoch());
            _channelWriter.TryWrite(bytes);
        }

        public void OnOrderTriggered(OrderId orderId)
        {
            var bytes = OrderTriggerSerializer.Serialize(orderId, _timeProvider.GetSecondsFromEpoch());
            _channelWriter.TryWrite(bytes);
        }

        public void OnTrade(OrderId incomingOrderId, OrderId restingOrderId, Price matchPrice, Quantity matchQuantiy, bool incomingOrderCompleted)
        {
            var bytes = FillSerializer.Serialize(restingOrderId, incomingOrderId, matchPrice, matchQuantiy, _timeProvider.GetSecondsFromEpoch(), incomingOrderCompleted);
            _channelWriter.TryWrite(bytes);
        }
    }
}
