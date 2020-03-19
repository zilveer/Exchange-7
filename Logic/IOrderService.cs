using APIModel.RequestModels;
using APIModel.ResponseModels;
using Entity;
using System.Collections.Generic;

namespace Logic
{
    public interface IOrderService
    {
        Orders FindById(int id);
        BusinessOperationResult<OrderResponseModel> PlaceOrder(int userId, OrderRequestModel orderRequestModel);
        BusinessOperationResult CancelOrder(int userId, int orderId);
        List<OrderResponseModel> GetOrdersForApi(int userId, int pageNumber = 1, int pageSize = 50);
    }
}
