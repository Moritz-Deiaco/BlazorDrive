using System.Text;
using BlazorDrive.App.Configuration;
using BlazorDrive.App.Database.Models;
using JWT.Algorithms;
using JWT.Builder;
using JWT.Exceptions;

namespace BlazorDrive.App.Services.Sessions;

public class IdentityService
{
    private readonly UserService UserService;
    private readonly CookieService CookieService;
    private readonly IHttpContextAccessor HttpContextAccessor;
    private readonly string Secret;
    
    private User? UserCache;

    public IdentityService(
        CookieService cookieService,
        UserService userService,
        IHttpContextAccessor httpContextAccessor,
        ConfigService configService)
    {
        CookieService = cookieService;
        UserService = userService;
        HttpContextAccessor = httpContextAccessor;

        Secret = configService.Get().JwtSecret;
    }

    public async Task<User?> Get()
    {
        try
        {
            if (UserCache != null)
                return UserCache;

            var token = "none";

            if (HttpContextAccessor.HttpContext != null)
            {
                var request = HttpContextAccessor.HttpContext.Request;

                if (request.Cookies.ContainsKey("token"))
                {
                    token = request.Cookies["token"];
                }
            }
            else
            {
                token = await CookieService.GetValue("token", "none");
            }

            if (token == "none")
            {
                return null;
            }

            if (string.IsNullOrEmpty(token))
                return null;

            var json = "";

            try
            {
                json = JwtBuilder.Create()
                    .WithAlgorithm(new HMACSHA256Algorithm())
                    .WithSecret(Secret)
                    .Decode(token);
            }
            catch (TokenExpiredException)
            {
                return null;
            }
            catch (SignatureVerificationException)
            {
                return null;
            }
            catch (Exception e)
            {
                return null;
            }

            // To make it easier to use the json data
            var data = new ConfigurationBuilder().AddJsonStream(
                new MemoryStream(Encoding.ASCII.GetBytes(json))
            ).Build();

            var userid = data.GetValue<int>("userid");
            var user = UserService.GetUserById(userid);

            if (user == null)
            {
                return null;
            }

            var iat = data.GetValue<long>("iat", -1);

            if (iat == -1)
            {
                return null;
            }

            var issuedAt = DateTimeOffset.FromUnixTimeSeconds(iat).DateTime;

            if (issuedAt < user.TokenValidTime.ToUniversalTime())
                return null;

            UserCache = user;
            return UserCache;
        }
        catch (Exception e)
        {
            return null;
        }
    }
    
    public User? GetSync()
    {
        try
        {
            if (UserCache != null)
                return UserCache;

            var token = "none";

            if (HttpContextAccessor.HttpContext != null)
            {
                var request = HttpContextAccessor.HttpContext.Request;

                if (request.Cookies.ContainsKey("token"))
                {
                    token = request.Cookies["token"];
                }
            }
            else
            {
                token = CookieService.GetValue("token", "none").Result;
            }

            if (token == "none")
            {
                return null;
            }

            if (string.IsNullOrEmpty(token))
                return null;

            var json = "";

            try
            {
                json = JwtBuilder.Create()
                    .WithAlgorithm(new HMACSHA256Algorithm())
                    .WithSecret(Secret)
                    .Decode(token);
            }
            catch (TokenExpiredException)
            {
                return null;
            }
            catch (SignatureVerificationException)
            {
                return null;
            }
            catch (Exception e)
            {
                return null;
            }

            // To make it easier to use the json data
            var data = new ConfigurationBuilder().AddJsonStream(
                new MemoryStream(Encoding.ASCII.GetBytes(json))
            ).Build();

            var userid = data.GetValue<int>("userid");
            var user = UserService.GetUserById(userid);

            if (user == null)
            {
                return null;
            }

            var iat = data.GetValue<long>("iat", -1);

            if (iat == -1)
            {
                return null;
            }

            var issuedAt = DateTimeOffset.FromUnixTimeSeconds(iat).DateTime;

            if (issuedAt < user.TokenValidTime.ToUniversalTime())
                return null;

            UserCache = user;
            return UserCache;
        }
        catch (Exception e)
        {
            return null;
        }
    }

    public string GetIp()
    {
        if (HttpContextAccessor.HttpContext == null)
            return "N/A";

        if(HttpContextAccessor.HttpContext.Request.Headers.ContainsKey("X-Real-IP"))
        {
            return HttpContextAccessor.HttpContext.Request.Headers["X-Real-IP"]!;
        }
        
        return HttpContextAccessor.HttpContext.Connection.RemoteIpAddress!.ToString();
    }
}