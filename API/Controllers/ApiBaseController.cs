using Entity;
using Logic;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace API.Controllers
{
    public abstract class ApiBaseController : ControllerBase
    {
        private readonly IUserService _userService;
        public ApiBaseController(IUserService userService)
        {
            _userService = userService;
        }

        protected Users GetUser()
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