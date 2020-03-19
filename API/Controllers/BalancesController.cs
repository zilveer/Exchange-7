using API.Controllers;
using APIModel.ResponseModels;
using Logic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace API
{
    [Route("api/[controller]")]
    [ApiController]
    public class BalancesController : ApiBaseController
    {
        private readonly IBalanceService _balanceService;
        public BalancesController(IBalanceService balanceService, IUserService userService) : base(userService)
        {
            _balanceService = balanceService;
        }

        [HttpGet]
        [Authorize("read-balance")]
        public IEnumerable<BalanceApiModel> Get()
        {
            var user = GetUser();
            return _balanceService.GetBalanceForApi(user.Id);
        }
    }
}
