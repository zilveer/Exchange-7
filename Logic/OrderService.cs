using APIModel.RequestModels;
using APIModel.ResponseModels;
using DataAccess.UnitOfWork;
using Entity;
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
        public OrderService(IUnitOfWork unitOfWork, IReadOnlyContext readOnlyContext)
        {
            _unitOfWork = unitOfWork;
            _readOnlyContext = readOnlyContext;
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
                    bor.Entity = Convert(entity);
                }
                return bor;
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
    }
}
