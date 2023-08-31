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
            if (_context.User.Where(a => a.UserName == user.UserName).Any())
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
                UserName = user.UserName,
                Email = user.Email,
                Password = passwordHash,
                RefreshToken = "2",
            };

            _context.Add(newUser);
            await _context.SaveChangesAsync();


            return Results.Ok("Success");
        }

        [HttpPost("Login")]
        public async Task<IResult> Login([FromBody] User loginUser)
        {
            IDictionary<string, string[]> validationErrors = new Dictionary<string, string[]>();

            var validEmail = await _context.User.Where(a => a.Email == loginUser.Email).SingleOrDefaultAsync();
            var validUser = await _context.User.Where(a => a.UserName == loginUser.Email).SingleOrDefaultAsync();
            if (validEmail != null)
            {
                PasswordHasher<User> passwordHasher = new PasswordHasher<User>();
                PasswordVerificationResult verificationResult = passwordHasher.VerifyHashedPassword(loginUser, validEmail.Password, loginUser.Password);
        
                if (verificationResult != PasswordVerificationResult.Success)
                {
                    validationErrors.Add("invalid_login", new string[] { "Invalid username or password" });
                }
            }
            else
            {
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
            }

            if (validationErrors.Count > 0)
            {
                return Results.ValidationProblem(validationErrors, "error");
            }

            var tokencontroller = new TokenController(_context, _configuration);
            if(validEmail != null)
            {
                AccessToken token = new AccessToken()
                {
                    access_token = "",
                    refresh_token = "",
                    userName = validEmail.UserName,
                    email = validEmail.Email,
                    id = validEmail.Id,
                };

                if(validEmail.RefreshToken == "2")
                {
                    validEmail.RefreshToken = "0";
                    UserAccount newUserAccount = new()
                    {
                        Username = validEmail.UserName,
                        Email = validEmail.Email,
                        UserId = validEmail.Id,
                    };
                    _context.Add(newUserAccount);
                    await _context.SaveChangesAsync();

                    _context.Update(validEmail);

                }

                return Results.Ok(token);
            }
            if (validUser != null)
            {
                AccessToken token = new AccessToken()
                {
                    access_token = "",
                    refresh_token = "",
                    userName = validUser.UserName,
                    email = validUser.Email,
                    id = validUser.Id,
                };

                if (validUser.RefreshToken == "2")
                {
                    validUser.RefreshToken = "0";
                    _context.Update(validUser);
                    UserAccount newUserAccount = new()
                    {
                        Username = validUser.UserName,
                        Email = validUser.Email,
                        UserId = validUser.Id,
                    };
                    _context.Add(newUserAccount);
                    await _context.SaveChangesAsync();
                }

                return Results.Ok(token);
            }


            return Results.Ok("logged in");
        }



        // POST: AuthController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: AuthController/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        [Authorize]
        [ServiceFilter(typeof(AuthTokenParseFilter))]
        [HttpGet("SecureData")]
        public IResult SecureData()
        {

            Console.WriteLine("should write");
            Console.WriteLine((HttpContext.Items["user_id"]));

            return Results.Ok("yay secure data");
        }

        // POST: AuthController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: AuthController/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: AuthController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }
    }
}
