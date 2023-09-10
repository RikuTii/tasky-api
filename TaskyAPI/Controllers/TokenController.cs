using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TaskyAPI.Data;
using TaskyAPI.Models;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using System.Text.Json.Nodes;
using System.Security.Cryptography;

namespace TaskyAPI.Controllers
{
    public class AccessToken
    {
        public string? userName { get; set; }
        public string? email { get; set; }
        public string? access_token { get; set; }
        public string? refresh_token { get; set; }
        public string? fcm_token { get; set; }
        public int? id { get; set; }
    }
    [ApiController]
    [Route("[controller]")]
    public class TokenController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;

        public TokenController(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomNumber);
                return Convert.ToBase64String(randomNumber);
            }
        }

        public async Task<AccessToken> CreateToken(User user)
        {
            user.RefreshToken = GenerateRefreshToken();
            _context.Update(user);
            await _context.SaveChangesAsync();

            var issuer = _configuration["Jwt:Issuer"];
            var audience = _configuration["Jwt:Audience"];
            var strKey = _configuration["Jwt:Key"];

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                        new Claim("Id", Guid.NewGuid().ToString()),
                        new Claim(JwtRegisteredClaimNames.Sub, user.Email),
                        new Claim(JwtRegisteredClaimNames.Email, user.Username),
                        new Claim(JwtRegisteredClaimNames.Jti,
                        Guid.NewGuid().ToString())
                    }),
                Expires = DateTime.UtcNow.AddMinutes(5),
                Issuer = issuer,
                Audience = audience,
                SigningCredentials = new SigningCredentials
                 (new SymmetricSecurityKey(Encoding.UTF8.GetBytes(strKey ?? "")),
                 SecurityAlgorithms.HmacSha256)
            };



            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            var stringToken = tokenHandler.WriteToken(token);


            var result = new AccessToken
            {
                access_token = stringToken,
                refresh_token = user.RefreshToken,
                email = user.Email,
                userName = user.Username,
            };

            return result;

        }

        public ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
        {
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = false, //you might want to validate the audience and issuer depending on your use case
                ValidateIssuer = false,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"] ?? "")),
                ValidateLifetime = false //here we are saying that we don't care about the token's expiration date
            };
            var tokenHandler = new JwtSecurityTokenHandler();
            SecurityToken securityToken;
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out securityToken);
            var jwtSecurityToken = securityToken as JwtSecurityToken;
            if (jwtSecurityToken == null || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                throw new SecurityTokenException("Invalid token");
            return principal;
        }


        public AccessToken RefreshToken(User user, string refreshToken, string accessToken)
        {

            var principal = GetPrincipalFromExpiredToken(accessToken);

            var sub = principal.Claims.Where(e => e.Type == ClaimTypes.NameIdentifier).FirstOrDefault();
            if (sub != null)
            {
                var value = sub.Value.ToString();
                if (user != null)
                {

                    var refresh = user.RefreshToken;
                    if (refresh == refreshToken)
                    {
                        user.RefreshToken = GenerateRefreshToken();

                        _context.Update(user);
                        _context.SaveChanges();
                        Console.WriteLine("refreshed");

                        var issuer = _configuration["Jwt:Issuer"];
                        var audience = _configuration["Jwt:Audience"];
                        var strKey = _configuration["Jwt:Key"];

                        var key = Base64UrlEncoder.DecodeBytes(strKey);

                        var tokenDescriptor = new SecurityTokenDescriptor
                        {
                            Subject = new ClaimsIdentity(new[]
                            {
                        new Claim("Id", Guid.NewGuid().ToString()),
                        new Claim(JwtRegisteredClaimNames.Sub, user.Email),
                        new Claim(JwtRegisteredClaimNames.Email, user.Username),
                        new Claim(JwtRegisteredClaimNames.Jti,
                        Guid.NewGuid().ToString())
                    }),
                            Expires = DateTime.UtcNow.AddMinutes(5),
                            Issuer = issuer,
                            Audience = audience,
                            SigningCredentials = new SigningCredentials
                             (new SymmetricSecurityKey(Encoding.UTF8.GetBytes(strKey ?? "")),
                             SecurityAlgorithms.HmacSha256)
                        };



                        var tokenHandler = new JwtSecurityTokenHandler();
                        var token = tokenHandler.CreateToken(tokenDescriptor);
                        var stringToken = tokenHandler.WriteToken(token);

                        var result = new AccessToken
                        {
                            access_token = stringToken,
                            refresh_token = user.RefreshToken,
                            email = user.Email,
                            userName = user.Username,
                        };

                        return result;

                    }
                }

            }

            return new AccessToken();

        }
    }
}
