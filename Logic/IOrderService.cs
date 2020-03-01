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
        List<OrderResponseModel> GetOrdersForApi(int userId, int pageNumber = 1, int pageSize = 50);
    }
}
