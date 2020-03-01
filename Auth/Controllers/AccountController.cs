using Auth.Models;
using IdentityServer4.Services;
using IdentityServer4.Stores;
using Logic;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;
using Util;

namespace Auth.Controllers
{
    public class AccountController : Controller
    {
        private const string STATE = "state";
        private const string AuthLogin = "AuthLogin";
        private readonly Logic.IAuthenticationService _authenticationService;
        private readonly IUserService _userService;
        private readonly IClientStore _clientStore;
        private readonly IIdentityServerInteractionService _identityServerInteractionService;
        private readonly IEventService _eventService;
        private readonly IDataProtectionProvider _dataProtectionProvider;
        private readonly ITimeProvider _timeProvider;
        public AccountController(Logic.IAuthenticationService authenticationService, IUserService userService, IClientStore clientStore, IIdentityServerInteractionService identityServerInteractionService, IEventService eventService, IDataProtectionProvider dataProtectionProvider, ITimeProvider timeProvider)
        {
            _authenticationService = authenticationService;
            _userService = userService;
            _clientStore = clientStore;
            _identityServerInteractionService = identityServerInteractionService;
            _eventService = eventService;
            _dataProtectionProvider = dataProtectionProvider;
            _timeProvider = timeProvider;
        }

        public IActionResult Login(string ReturnUrl)
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(Login login)
        {
            if (ModelState.IsValid && login != null)
            {
                var authenticationResult = _authenticationService.Authenticate(login.EmailOrMobile, login.Password);

                if (authenticationResult.ErrorCode == 0)
                {
                    var twoFactorEnabled = _authenticationService.IsTwoFactorEnabled(authenticationResult.Entity.UserId);
                    if (twoFactorEnabled)
                    {
                        var state = new AuthTempState { LoginTime = _timeProvider.GetUtcDateTime(), UserId = authenticationResult.Entity.UserId };
                        var jsonString = JsonSerializer.Serialize(state);
                        var protector = _dataProtectionProvider.CreateProtector(AuthLogin);
                        var protectedString = protector.Protect(jsonString);
                        Response.Cookies.Append(STATE, protectedString, new CookieOptions() { HttpOnly = true, Secure = true, Expires = DateTimeOffset.UtcNow.AddHours(1) });
                        return RedirectToAction(nameof(VerifyTOTP).ToLower(), new { returnUrl = login?.ReturnUrl });
                    }
                }
                else
                {
                    ModelState.AddModelError("", authenticationResult.ErrorMessage);
                }
            }
            return View(login);
        }


        public IActionResult VerifyTOTP(string returnUrl)
        {
            if (Request.Cookies.ContainsKey(STATE))
            {
                IDataProtector dataProtector = _dataProtectionProvider.CreateProtector(AuthLogin);
                var jsonString = dataProtector.Unprotect(Request.Cookies[STATE]);
                var state = JsonSerializer.Deserialize<AuthTempState>(jsonString);
                if (state.LoginTime < _timeProvider.GetUtcDateTime().AddMinutes(-30))
                {
                    return RedirectToAction(nameof(Login));
                }
                return View();
            }
            else
            {
                return RedirectToAction(nameof(Login));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> VerifyTOTP(VerifyTOTPModel verifyTOTPModel)
        {
            if (ModelState.IsValid)
            {
                if (Request.Cookies.ContainsKey(STATE))
                {
                    IDataProtector dataProtector = _dataProtectionProvider.CreateProtector(AuthLogin);
                    var jsonString = dataProtector.Unprotect(Request.Cookies[STATE]);
                    var state = JsonSerializer.Deserialize<AuthTempState>(jsonString);
                    if (state.LoginTime < _timeProvider.GetUtcDateTime().AddMinutes(-30))
                    {
                        return RedirectToAction(nameof(Login));
                    }

                    int userId = state.UserId;
                    var otpResult = _authenticationService.VerifyTotp(userId, verifyTOTPModel.TOTP);
                    if (otpResult.ErrorCode == 0)
                    {
                        var user = _userService.GetUser(userId);
                        var claims = new List<Claim>()
                        {
                            new Claim("sub", user.UniqueId.ToString().ToLower()),
                            new Claim("name", user.UniqueId.ToString().ToLower())
                        };

                        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                        var authProperties = new AuthenticationProperties
                        {
                            AllowRefresh = true,
                            ExpiresUtc = DateTimeOffset.Now.AddDays(1),
                            IsPersistent = true,
                        };
                        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), authProperties);
                        var context = await _identityServerInteractionService.GetAuthorizationContextAsync(verifyTOTPModel.ReturnUrl);
                        if (context != null)
                        {
                            var client = await _clientStore.FindClientByIdAsync(context.ClientId);
                            if (client != null && client.RequirePkce)
                            {
                                ViewBag.RedirectUrl = verifyTOTPModel.ReturnUrl;
                                return View("Redirect");
                            }
                            else
                            {
                                // we can trust model.ReturnUrl since GetAuthorizationContextAsync returned non-null
                                return Redirect(verifyTOTPModel.ReturnUrl);
                            }
                        }

                        // request for a local page
                        if (Url.IsLocalUrl(verifyTOTPModel.ReturnUrl))
                        {
                            return Redirect(verifyTOTPModel.ReturnUrl);
                        }
                        else if (string.IsNullOrEmpty(verifyTOTPModel.ReturnUrl))
                        {
                            return Redirect("~/");
                        }
                    }
                    else
                    {
                        ModelState.AddModelError("", otpResult.ErrorMessage);
                    }
                }
                else
                {
                    return RedirectToAction(nameof(Login));
                }
            }
            return View();
        }
    }
}
