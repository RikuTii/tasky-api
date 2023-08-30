using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskyAPI.Data;
using TaskyAPI.Models;

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

            UserAccount newUserAccount = new()
            {
                Username = "jaakko",
                Email = "test@test123.com",
            };



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
                    PasswordVerificationResult verificationResult = passwordHasher.VerifyHashedPassword(loginUser, loginUser.Password, loginUser.Password);
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
                AccessToken token = await tokencontroller.CreateToken(validEmail);
                return Results.Ok(token);
            }
            if (validUser != null)
            {
                AccessToken token = await tokencontroller.CreateToken(validUser);
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
        [HttpGet("SecureData")]
        public IResult SecureData()
        {

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
