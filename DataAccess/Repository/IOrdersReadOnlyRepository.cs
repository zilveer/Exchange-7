using APIModel.ResponseModels;
using Entity;
using System.Collections.Generic;
using System.Linq;

namespace DataAccess.Repository
{
    public interface IOrdersReadOnlyRepository : IReadOnlyRepository<Orders>
    {
        IQueryable<Orders> GetRecentOrders(int userId, short market);
        List<Orders> GetOrders(int userId, int pageNumber = 1, int pageSize = 50);
    }
}
