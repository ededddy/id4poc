using IdentityModel;
using IdentityServer.Entities;
using IdentityServer4;
using IdentityServer4.Events;
using IdentityServer4.Services;
using IdentityServer4.Test;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Facebook;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace IdentityServer.Controllers;


public class FacebookController : Controller
{
    private readonly IConfiguration _configuration;
    private readonly IIdentityServerInteractionService _interaction;
    private readonly ILogger<FacebookController> _logger;
    private readonly IEventService _events;
    private readonly TestUserStore _user;
    private readonly IdentityServerTools _identityServerTools;
    public FacebookController(IConfiguration configuration,
        ILogger<FacebookController> logger,
        IdentityServerTools identityServerTools,
        IEventService events,
        TestUserStore user,
        IIdentityServerInteractionService interaction)
    {
        _configuration = configuration;
        _logger = logger;
        _interaction = interaction;
        _user = user;
        _events = events;
        _identityServerTools = identityServerTools;
    }

    private bool IsValidReturnUrl(string provider, string url)
    {
        var links = _configuration.GetSection($"ValidReturn:{provider}").Get<string[]>();
        return true;
        //return links.Any(s => s == url);
    }

    [HttpGet]
    public ChallengeResult Index(string returnUrl)
    {
        var callbackUrl = Url.Action("ExternalLoginCallback");
        var provider = "Facebook";
        if( !IsValidReturnUrl(provider, returnUrl))
        {
            throw new Exception("Invalid return URL");
        }

        var props = new AuthenticationProperties
        {
            RedirectUri = callbackUrl,
            Items =
            {
                { "scheme", provider },
                { "returnUrl", returnUrl }
            }
        };

        return Challenge(props, provider);
    }

    [HttpGet]
    public async Task<IActionResult> ExternalLoginCallback ()
    {
        // read external identity from the temporary cookie
        var result = await HttpContext.AuthenticateAsync(IdentityServerConstants.ExternalCookieAuthenticationScheme);
        if (result?.Succeeded != true)
        {
            throw new Exception("External authentication error");
        }
        
        if (_logger.IsEnabled(LogLevel.Debug))
        {
            var externalClaims = result.Principal.Claims.Select(c => $"{c.Type}: {c.Value}");
            _logger.LogDebug("External claims: {@claims}", externalClaims);
        }
        var (user, provider, providerUserId, claims) = FindUserFromExternal(result);
        if(user == null)
        {
            user = _user.AutoProvisionUser(provider, providerUserId, claims.ToList());
        //    throw new Exception("Invalid User");
        }

        var email = claims.FirstOrDefault(c => c.Type == ClaimTypes.Email).Value;
        var additionalLocalClaims = new List<Claim>(){ 
            new Claim(ClaimTypes.Email, email),
            new Claim("role.apione", "apioneadmin"),
            new Claim("scope.claim", "apioneclaim")
        };

        // var localSignInProps = new AuthenticationProperties();
        //var issuser = new IdentityServerUser(user.SubjectId)
        //{
        //    DisplayName = user.Username,
        //    IdentityProvider = provider,
        //    AdditionalClaims = additionalLocalClaims
        //};
        // await HttpContext.SignInAsync(issuser, localSignInProps);

        var jwt = await _identityServerTools.IssueClientJwtAsync(
            clientId : "userjs",
            lifetime : 3600,
            audiences : new List<string> { "ApiOne" },
            scopes: new List<string> { "ApiOne.Read"},
            additionalClaims : additionalLocalClaims
        );

        await HttpContext.SignOutAsync(IdentityServerConstants.ExternalCookieAuthenticationScheme);

        var returnUrl = result.Properties.Items["returnUrl"] ?? "~/";

        var context = await _interaction.GetAuthorizationContextAsync(returnUrl);
        await _events.RaiseAsync(new UserLoginSuccessEvent(provider, providerUserId, user.SubjectId, user.Username, true, context?.Client.ClientId));

        return Redirect($"{returnUrl}?tok={jwt}");
    }

    private (TestUser user, string provider, string providerUserId, IEnumerable<Claim> claims) FindUserFromExternal(AuthenticateResult result)
    {
        var extUser = result.Principal;

        var userIdClaims = extUser.FindFirst(JwtClaimTypes.Subject)
                           ?? extUser.FindFirst(ClaimTypes.NameIdentifier)
                           ?? throw new Exception("Unknown user Id");

        var claims = extUser.Claims.ToList();
        claims.Remove(userIdClaims);

        var provider = result.Properties.Items["scheme"];
        var providerUserId = userIdClaims.Value;
        // TODO: find user from db

        return (null, provider, providerUserId, claims);
    }
}