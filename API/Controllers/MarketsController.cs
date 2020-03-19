using APIModel.ResponseModels;
using Logic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace API
{
    [Route("api/[controller]")]
    [ApiController]
    public class MarketsController : ControllerBase
    {
        private readonly IMarketService _marketService;
        public MarketsController(IMarketService marketService)
        {
            _marketService = marketService;
        }


        [HttpGet]
        public List<MarketApiModel> Get()
        {
            return _marketService.GetMarketsForApi();
        }
    }
}
