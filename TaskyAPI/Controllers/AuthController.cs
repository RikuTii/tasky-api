using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Primitives;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using TaskyAPI.Data;
using TaskyAPI.Models;
using TaskyAPI.Middleware;

namespace TaskyAPI.Controllers
{
    public class AuthController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthController(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [HttpPost("Register")]
        public async Task<IResult> Register([FromBody] User user)
        {
            IDictionary<string, string[]> validationErrors = new Dictionary<string, string[]>();

            if (_context.User.Where(a => a.Email == user.Email).Any())
            {
                validationErrors.Add("email_taken", new string[] { "Email is already in use" });
            }
            if (_context.User.Where(a => a.Username == user.Username).Any())
            {
                validationErrors.Add("user_taken", new string[] { "Username is already in use" });
            }

            if (validationErrors.Count > 0)
            {
                return Results.ValidationProblem(validationErrors, "error");
            }

            PasswordHasher<User> passwordHasher = new PasswordHasher<User>();
            string passwordHash = passwordHasher.HashPassword(user, user.Password);

            User newUser = new()
            {
                Username = user.Username,
                Email = user.Email,
                Password = passwordHash,
                RefreshToken = "",
            };

            _context.Add(newUser);
            await _context.SaveChangesAsync();


            return Results.Ok("Success");
        }



        [Authorize]
        [ServiceFilter(typeof(AuthTokenParseFilter))]
        [HttpPost("UpdateDevice")]
        public async Task<IResult> UpdateDevice([FromBody] UserDevice device)
        {
            var accountId = HttpContext.Items["account_id"];
            if (accountId != null)
            {
                int account_id = (int)accountId;

                UserDevice? latestDevice = await _context.UserDevice.Where(e => e.AccountId == account_id).OrderBy(e => e.LastActive).FirstOrDefaultAsync();
                if(latestDevice != null)
                {
                    latestDevice.FcmToken = device.FcmToken;
                    _context.Update(latestDevice);
                    await _context.SaveChangesAsync();
                }
 
                return Results.Ok();
            }

            return Results.Unauthorized();

        }

        [HttpPost("Login")]
        public async Task<IResult> Login([FromBody] User loginUser)
        {
            IDictionary<string, string[]> validationErrors = new Dictionary<string, string[]>();

            User? validEmail = await _context.User.Where(a => a.Email == loginUser.Email).FirstOrDefaultAsync();
            User? validUser;
            if (validEmail != null)
            {
                validUser = validEmail;
            }
            else
            {
                validUser = await _context.User.Where(a => a.Username == loginUser.Email).FirstOrDefaultAsync();
            }


            if (validUser != null)
            {
                PasswordHasher<User> passwordHasher = new PasswordHasher<User>();
                PasswordVerificationResult verificationResult = passwordHasher.VerifyHashedPassword(loginUser, validUser.Password, loginUser.Password);
                if (verificationResult != PasswordVerificationResult.Success)
                {
                    validationErrors.Add("invalid_login", new string[] { "Invalid username or password" });
                }
            }
            else
            {
                validationErrors.Add("invalid_login", new string[] { "Invalid username or password" });
            }


            if (validationErrors.Count > 0 || validUser == null)
            {
                return Results.ValidationProblem(validationErrors, "error");
            }

            TokenController tokencontroller = new TokenController(_context, _configuration);

            AccessToken token = new AccessToken()
            {
                access_token = "",
                refresh_token = "",
                userName = validUser.Username,
                email = validUser.Email,
                id = validUser.Id,
                fcm_token = "pending"
            };

            if (validUser.RefreshToken == "")
            {
                validUser.RefreshToken = tokencontroller.GenerateRefreshToken();
                UserAccount newUserAccount = new()
                {
                    Username = validUser.Username,
                    Email = validUser.Email,
                    UserId = validUser.Id,
                };
                _context.Add(newUserAccount);

                validUser.Accounts = new List<UserAccount>
                    {
                        newUserAccount
                    };
                _context.Update(validUser);

                await _context.SaveChangesAsync();
                if (loginUser.Accounts.Count > 0 && loginUser.Accounts.ElementAt(0)?.Devices?.Count > 0)
                {
                    UserAccount deviceAccount = loginUser.Accounts.ElementAt(0);
                    if(deviceAccount != null && deviceAccount.Devices != null)
                    {
                        UserDevice loginDevice = deviceAccount.Devices.ElementAt(0);
                        if(loginDevice != null)
                        {
                            UserDevice newDevice = new()
                            {
                                Type = loginDevice.Type,
                                LastActive = DateTime.Now,
                                AccountId = newUserAccount.Id
                            };

                            _context.Add(newDevice);
                            await _context.SaveChangesAsync();
                        }                 
                    }
                  
                }
             

                var account = _context.UserAccount.Where(e => e.UserId == validUser.Id).FirstOrDefault();
                if (account != null)
                {
                    token.id = account.Id;
                    token.userName = account.Username;
                }
            }
            else
            {

                //temporary single account
                var account = await _context.UserAccount.Where(e => e.UserId == validUser.Id).Include(e => e.Devices!.OrderBy(e => e.LastActive)).FirstAsync();
                if (account != null)
                {
                    if (account.Devices != null && account.Devices.Count > 0)
                    {
                        UserDevice? firstDevice = account.Devices.FirstOrDefault();
                        if(firstDevice != null)
                        {
                            if(firstDevice.FcmToken != null)
                            {
                                token.fcm_token = firstDevice.FcmToken;
                            }
                        }
                    }
   
                    token.id = account.Id;
                    token.userName = account.Username;
                }
            }

            return Results.Ok(token);

        }
    }
}
