using APIModel.RequestModels;
using APIModel.ResponseModels;
using DataAccess.UnitOfWork;
using Entity;
using Infrastructure;
using OrderMatcher;
using System;
using System.Collections.Generic;
using System.Linq;
using Util;

namespace Logic
{
    public class OrderService : IOrderService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IReadOnlyContext _readOnlyContext;
        private readonly IQueueProducer _queueProducer;
        public OrderService(IUnitOfWork unitOfWork, IReadOnlyContext readOnlyContext, IQueueProducer queueProducer)
        {
            _unitOfWork = unitOfWork;
            _readOnlyContext = readOnlyContext;
            _queueProducer = queueProducer;
        }

        public Orders FindById(int id)
        {
            return _readOnlyContext.OrdersRepository.Find(id);
        }

        public List<OrderResponseModel> GetOrdersForApi(int userId, int pageNumber = 1, int pageSize = 50)
        {
            return _readOnlyContext.OrdersRepository.GetOrders(userId, pageNumber, pageSize).Select(x => Convert(x)).ToList();
        }

        public BusinessOperationResult<OrderResponseModel> PlaceOrder(int userId, OrderRequestModel orderRequestModel)
        {
            if (orderRequestModel == null)
                throw new ArgumentNullException(nameof(orderRequestModel));

            using (var uow = _unitOfWork.GetNewUnitOfWork())
            {
                var icebergQuantity = orderRequestModel.IcebergQuantity.HasValue ? orderRequestModel.IcebergQuantity.Value.ToSatoshi() : 0;
                var result = uow.OrdersRepository.PlaceOrder(userId, orderRequestModel.MarketId.Value, orderRequestModel.IsBuy.Value, orderRequestModel.Quantity.Value.ToSatoshi(), orderRequestModel.Rate.Value, orderRequestModel.StopRate.Value, (short)orderRequestModel.OrderType.Value, (short)orderRequestModel.OrderCondition.Value, orderRequestModel.CancelOn, icebergQuantity);
                var bor = new BusinessOperationResult<OrderResponseModel> { ErrorCode = result.ErrorCode, ErrorMessage = result.ErrorMessage, Id = result.OrderId };
                if (result.ErrorCode == 0)
                {
                    var entity = uow.OrdersRepository.Find(result.OrderId);
                    SendNewOrderRequest(entity);
                    bor.Entity = Convert(entity);
                }
                return bor;
            }
        }

        public BusinessOperationResult CancelOrder(int userId, int orderId)
        {
            using (var uow = _unitOfWork.GetNewUnitOfWork())
            {
                var order = uow.OrdersRepository.Find(orderId);
                if (order != null && order.UserId == userId)
                {
                    if (order.OrderStatus == Entity.Partials.OrderStatus.Accepted || order.OrderStatus == Entity.Partials.OrderStatus.Received)
                    {
                        SendCancelRequest(order.Id);
                        return new BusinessOperationResult { ErrorMessage = "Cancel request accepted." };
                    }
                    else if (order.OrderStatus == Entity.Partials.OrderStatus.Cancelled)
                    {
                        return new BusinessOperationResult { ErrorCode = 1, ErrorMessage = "Order is already cancelled." };
                    }
                    else if (order.OrderStatus == Entity.Partials.OrderStatus.Filled)
                    {
                        return new BusinessOperationResult { ErrorCode = 1, ErrorMessage = "Order is already filled." };
                    }
                    else if (order.OrderStatus == Entity.Partials.OrderStatus.Rejected)
                    {
                        return new BusinessOperationResult { ErrorCode = 1, ErrorMessage = "Order is already rejected." };
                    }
                    else
                    {
                        return new BusinessOperationResult { ErrorCode = 1, ErrorMessage = "Order can not be cancelled." };
                    }
                }
                else
                {
                    return new BusinessOperationResult { ErrorCode = 1, ErrorMessage = "Order not found" };
                }
            }
        }

        private OrderResponseModel Convert(Orders x)
        {
            return new OrderResponseModel
            {
                CancelOn = x.CancelOn,
                CreatedOn = x.CreatedOn,
                Fee = x.Fee.ToCoin(),
                FeeCurrencyId = x.FeeCurrencyId,
                Id = x.Id,
                IsBuy = x.Side,
                LockedBalance = x.LockedBalance.ToCoin(),
                MarketId = x.MarketId,
                OrderCondition = x.OrderCondition,
                OrderStatus = x.OrderStatus,
                OrderType = x.OrderType,
                Quantity = x.Quantity.ToCoin(),
                QuantityExecuted = x.QuantityExecuted.ToCoin(),
                QuantityRemaining = x.QuantityRemaining.ToCoin(),
                Rate = x.Rate,
                StopRate = x.StopRate,
                TradeFeeId = x.TradeFeeId,
                IcebergQuantity = x.IcebergQuantity.HasValue ? x.IcebergQuantity.Value.ToCoin() : (decimal?)null
            };
        }

        private void SendNewOrderRequest(Orders order)
        {
            var orderWrapper = new OrderWrapper();
            orderWrapper.Order = new Order();

            orderWrapper.Order.OrderId = order.Id;
            orderWrapper.Order.IsBuy = order.Side;
            orderWrapper.Order.Price = order.Rate;
            orderWrapper.Order.OpenQuantity = order.QuantityRemaining;
            orderWrapper.Order.IsStop = order.StopRate != 0;
            orderWrapper.OrderCondition = (OrderCondition)((byte)order.OrderCondition);
            orderWrapper.StopPrice = order.StopRate;
            orderWrapper.TipQuantity = order.IcebergQuantity > 0 ? order.Quantity : 0;
            orderWrapper.TotalQuantity = order.IcebergQuantity ?? 0;
            if (order.CancelOn.HasValue)
            {
                DateTime Jan1970 = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                orderWrapper.Order.CancelOn = (int)order.CancelOn.Value.Subtract(Jan1970).TotalSeconds;
            }

            var bytes = OrderSerializer.Serialize(orderWrapper);
            _queueProducer.Produce(bytes);
        }

        private void SendCancelRequest(int orderId)
        {
            var cancelRequest = new CancelRequest() { OrderId = orderId };
            var bytes = CancelRequestSerializer.Serialize(cancelRequest);
            _queueProducer.Produce(bytes);
        }
    }
}
