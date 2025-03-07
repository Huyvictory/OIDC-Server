using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using OidcServer.Helpers;
using OidcServer.Models;
using OidcServer.Repository;

namespace OidcServer.Controllers;

public class AuthorizeController : Controller
{
    private readonly IUserRepository _userRepository;
    private readonly ICodeItemRepository _codeItemRepository;

    public AuthorizeController(IUserRepository userRepository, ICodeItemRepository codeItemRepository)
    {
        _userRepository = userRepository;
        _codeItemRepository = codeItemRepository;
    }

    // GET
    public IActionResult Index(AuthenticationRequestModel authenticationRequest)
    {
        return View(authenticationRequest);
    }

    [HttpPost]
    public IActionResult Authorize(AuthenticationRequestModel authenticationRequest, string user, string[] scopes)
    {
        if (_userRepository.FindByName(user) == null)
        {
            return View("UserNotFound");
        }

        string code = CodeGenerator();

        var model = new CodeFlowResponseViewModel()
        {
            Code = code,
            State = authenticationRequest.State,
            RedirectUri = authenticationRequest.RedirectUri,
        };

        _codeItemRepository.Add(code,
            new CodeItem()
            {
                AuthenticationRequest = authenticationRequest,
                User = user,
                Scopes = scopes
            });

        return View("SubmitForm", model);
    }

    [Route("oauth/token")]
    [HttpPost]
    public IActionResult ReturnTokens(string grant_type, string code, string redirect_uri)
    {
        if (grant_type != "authorization_code")
        {
            return BadRequest();
        }

        var codeItem = _codeItemRepository.FindByCode(code);

        if (codeItem == null)
        {
            return BadRequest();
        }

        if (codeItem.AuthenticationRequest.RedirectUri != redirect_uri)
        {
            return BadRequest();
        }

        _codeItemRepository.Delete(code);

        var model = new AuthenticationResponseModel()
        {
            AccessToken = GenerateAccessToken(codeItem.User, string.Join(" ", codeItem.Scopes),
                codeItem.AuthenticationRequest.ClientId,
                codeItem.AuthenticationRequest.Nonce, JwkLoader.LoadFromDefault()),
            RefreshToken = GenerateRefreshToken(),
            TokenType = "Bearer",
            State = codeItem.AuthenticationRequest.State,
            ExpiresIn = 3600,
            IdToken = GenerateIdToken(codeItem.User, codeItem.AuthenticationRequest.ClientId,
                codeItem.AuthenticationRequest.Nonce, JwkLoader.LoadFromDefault()),
        };

        return Json(model);
    }

    private string GenerateAccessToken(string userId, string scope, string audience, string nonce,
        JsonWebKey jsonWebKey)
    {
        // access_token can be the same as id_token, but here we might have different values for expirySeconds so we use 2 different functions

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, userId),
            new("scope",
                scope)
        };
        var idToken = JwtGenerator.GenerateJWTToken(
            20 * 60,
            "https://localhost:7153/",
            audience,
            nonce,
            claims,
            jsonWebKey
        );

        return idToken;
    }

    private string GenerateIdToken(string userId, string audience, string nonce, JsonWebKey jsonWebKey)
    {
        // https://openid.net/specs/openid-connect-core-1_0.html#IDToken
        // we can return some claims defined here: https://openid.net/specs/openid-connect-core-1_0.html#StandardClaims
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, userId)
        };

        var idToken = JwtGenerator.GenerateJWTToken(
            20 * 60,
            "https://localhost:7153/",
            audience,
            nonce,
            claims,
            jsonWebKey
        );

        return idToken;
    }

    private static string GenerateRefreshToken()
    {
        return Guid.NewGuid().ToString("N");
    }

    private string CodeGenerator()
    {
        return Guid.NewGuid().ToString();
    }

    [NonAction]
    private static void ValidateAuthenticateRequestModel(AuthenticationRequestModel authenticateRequest)
    {
        ArgumentNullException.ThrowIfNull(authenticateRequest, nameof(authenticateRequest));

        if (string.IsNullOrEmpty(authenticateRequest.ClientId))
        {
            throw new Exception("client_id required");
        }

        if (string.IsNullOrEmpty(authenticateRequest.ResponseType))
        {
            throw new Exception("response_type required");
        }

        if (string.IsNullOrEmpty(authenticateRequest.Scope))
        {
            throw new Exception("scope required");
        }

        if (string.IsNullOrEmpty(authenticateRequest.RedirectUri))
        {
            throw new Exception("redirect_uri required");
        }
    }
}