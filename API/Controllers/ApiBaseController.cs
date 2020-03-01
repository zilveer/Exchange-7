using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DataAccess.UnitOfWork;
using Entity;
using Logic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public abstract class ApiBaseController : ControllerBase
    {
        private readonly IUserService _userService;
        public ApiBaseController(IUserService userService)
        {
            _userService = userService;
        }

        public Users GetUser()
        {
            var subject = User?.Claims.FirstOrDefault(x => x.Issuer == "https://localhost:44326" && x.OriginalIssuer == "https://localhost:44326" && x.Type == "sub")?.Value;
            if (!string.IsNullOrWhiteSpace(subject))
            {
                return _userService.GetByUniqueId(subject);
            }
            return null;
        }
    }
}