using API.Controllers;
using APIModel.RequestModels;
using APIModel.ResponseModels;
using Logic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace API
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class OrdersController : ApiBaseController
    {
        private readonly IOrderService _orderService;
        public OrdersController(IOrderService orderService, IUserService userService) : base(userService)
        {
            _orderService = orderService;
        }

        [HttpGet]
        [Authorize("read-orders")]
        public List<OrderResponseModel> Get([FromQuery]PageModel pageModel)
        {
            var user = GetUser();
            if (user != null)
            {
                return _orderService.GetOrdersForApi(user.Id, pageModel?.PageNumber ?? 1, pageModel?.PageSize ?? 50);
            }
            return null;
        }

        [HttpPost]
        [Authorize("post-orders")]
        [ProducesResponseType(typeof(OrderResponseModel), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public ActionResult<OrderResponseModel> Post([FromBody]OrderRequestModel order)
        {
            var user = GetUser();
            if (user != null)
            {
                var result = _orderService.PlaceOrder(user.Id, order);
                if (result.ErrorCode != 0)
                {
                    ModelState.AddModelError("", result.ErrorMessage);
                    return BadRequest(ModelState);
                }
                return result.Entity;
            }
            return null;
        }
    }
}
