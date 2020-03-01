using APIModel.ResponseModels;
using Entity;
using Entity.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using Util;

namespace DataAccess.Repository
{
    public class OrdersRepository : Repository<Orders>, IOrdersReadOnlyRepository, IOrdersRepository
    {
        private readonly DbContext _dbContext;
        public OrdersRepository(DbSet<Orders> dbSet, DbContext dbContext) : base(dbSet)
        {
            _dbContext = dbContext;
        }

        public IQueryable<Orders> GetRecentOrders(int userId, short market)
        {
            return DbSet.Where(x => x.UserId == userId && x.MarketId == market);
        }

        public PlaceOrderResult PlaceOrder(int userId, short marketId, bool side, long quantity, decimal rate, decimal stopRate, short orderType, short orderCondition, DateTime? cancelOn, long? icebergQuantity)
        {
            string sql = "CALL place_order(@userId, @marketId, @side, @quantity, @rate, @stopRate, @orderType, @orderCondition, @cancelOn, @icebergQuantity, @errorCode, @errorMessage, @orderId)";
            var pUserId = InParam("@userId", userId);
            var pMarketId = InParam("@marketId", marketId);
            var pSide = InParam("@side", side);
            var pQuantity = InParam("@quantity", quantity);
            var pRate = InParam("@rate", rate);
            var pStopRate = InParam("@stopRate", stopRate);
            var pOrderCondition = InParam("@orderCondition", orderCondition);
            var pCancelOn = InParam("@cancelOn", cancelOn);
            var pIcebergQuantity = InParam("@icebergQuantity", icebergQuantity);
            var pOrderType = InParam("@orderType", orderType);
            var pErrorCode = InOutParamInt("@errorCode");
            var pErrorMessage = InOutParamString("@errorMessage");
            var pOrderId = InOutParamInt("@orderId");
            _dbContext.Database.ExecuteSqlCommand(sql, pUserId, pMarketId, pSide, pQuantity, pRate, pStopRate, pOrderType, pOrderCondition, pCancelOn, pIcebergQuantity, pErrorCode, pErrorMessage, pOrderId);
            return new PlaceOrderResult()
            {
                ErrorCode = (int)pErrorCode.Value,
                ErrorMessage = pErrorMessage.Value == DBNull.Value ? null : (string)pErrorMessage.Value,
                OrderId = pOrderId.Value == DBNull.Value ? null : (int?)pOrderId.Value
            };
        }

        public List<Orders> GetOrders(int userId, int pageNumber = 1, int pageSize = 50)
        {
            return DbSet.Where(x => x.UserId == userId).OrderBy(x => x.CreatedOn).Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();
        }
    }
}
