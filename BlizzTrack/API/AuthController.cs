using AspNet.Security.OAuth.BattleNet;
using BlizzTrack.Helpers;
using Core.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.FeatureManagement.Mvc;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace BlizzTrack.API
{
    [ApiExplorerSettings(IgnoreApi = true)]
    [Route("Auth")]
    [ApiController]
    [FeatureGate(nameof(FeatureFlags.UserAuth))]
    [Authorize]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly ILogger<AuthController> _logger;
        private readonly IConfiguration _config;

        public AuthController(UserManager<User> userManager, ILogger<AuthController> logger, IConfiguration config)
        {
            _userManager = userManager;
            _logger = logger;
            _config = config;
        }

        [AllowAnonymous]
        [HttpGet("Login")]
        public async Task Login(string returnUrl = "/")
        {
            await HttpContext.ChallengeAsync(BattleNetAuthenticationDefaults.AuthenticationScheme, new AuthenticationProperties()
            {
                RedirectUri = new PathString("/Auth/ExternalAuthLogin").Add(QueryString.Create("returnUrl", "/")),
            });
        }

        [HttpGet("Logout")]
        public async Task<ActionResult> Logout()
        {
            await HttpContext.SignOutAsync();

            return Redirect("/");
        }

        [AllowAnonymous]
        [HttpGet("ExternalAuthLogin")]
        public async Task<ActionResult> ExternalAuthLogin(string returnUrl = "/")
        {
            await Task.Delay(TimeSpan.FromSeconds(1));

            var cur = HttpContext.User.Claims;

            var name = cur.First(x => x.Type == ClaimTypes.Name).Value;
            var user = new User
            {
                AccessToken = cur.First(x => x.Type == "urn:bnet:access_token").Value,
                Id = cur.First(x => x.Type == ClaimTypes.NameIdentifier).Value,
                Email = $"{name.Replace("#", "-")}@battle.net", // We don't want user emails
                UserName = name.Replace("#", "-"),
                BattleTag = name
            };

            var currentUser = await _userManager.FindByIdAsync(user.Id);
            if (currentUser != null)
            {
                currentUser.LockoutEnabled = false;
                currentUser.AccessToken = user.AccessToken;
                currentUser.BattleTag = user.BattleTag;
                currentUser.Email = $"{name.Replace("#", "-")}@battle.net";
                currentUser.UserName = name.Replace("#", "-");

                user = currentUser;

                await _userManager.UpdateAsync(currentUser);
            }
            else
            {
                user.EmailConfirmed = true;
                user.LockoutEnabled = false;
                await _userManager.CreateAsync(user);
            }

            await HttpContext.SignOutAsync();

            if (!await _userManager.IsInRoleAsync(user, "User"))
                await _userManager.AddToRoleAsync(user, "User");

            if (user.Id == _config.GetValue<string>("root_user", ""))
            {
                if (!await _userManager.IsInRoleAsync(user, "Admin"))
                    await _userManager.AddToRoleAsync(user, "Admin");
            }

            var identity = new ClaimsIdentity(CookieAuthenticationDefaults.AuthenticationScheme, ClaimTypes.Name, ClaimTypes.Role);

            identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, user.Id));
            identity.AddClaim(new Claim(ClaimTypes.Name, user.UserName));
            identity.AddClaim(new Claim("urn:battle_tag", user.BattleTag));

            foreach (var role in await _userManager.GetRolesAsync(user))
            {
                identity.AddClaim(new Claim(ClaimTypes.Role, role));
            }

            var f = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, f, new AuthenticationProperties
            {
                IsPersistent = true
            });

            return Redirect("/");
        }
    }
}