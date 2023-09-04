using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using System.Security.Claims;
using System.Text.Json.Nodes;
using TaskyAPI.Data;
using TaskyAPI.Middleware;
using TaskyAPI.Models;

namespace TaskyAPI.Controllers
{
    [Authorize]
    [ServiceFilter(typeof(AuthTokenParseFilter))]
    [Route("[controller]")]
    public class ProfileController : Controller
    {
        private readonly ApplicationDbContext _context;



        public ProfileController(ApplicationDbContext context)
        {
            _context = context;
        }


        [HttpGet("profile")]
        public async Task<IResult> GetProfile()
        {
            var accountId = HttpContext.Items["account_id"];
            if(accountId != null)
            {
                int acccount_id = (int)accountId;

                UserAccount account = await _context.UserAccount.Where(e => e.Id == acccount_id).FirstAsync();
                if(account != null)
                {
                    return Results.Ok(account);
                }

            }

            return Results.NotFound();
        }

        [HttpPost("update")]
        public async Task<IResult> Update([FromBody] UserAccount updateAccount)
        {
            var accountId = HttpContext.Items["account_id"];
            if (accountId != null)
            {
                int acccount_id = (int)accountId;
                UserAccount account = await _context.UserAccount.Where(e => e.Id == acccount_id).FirstAsync();
                if (account != null)
                {
                    account.FirstName = updateAccount.FirstName;
                    account.LastName = updateAccount.LastName;
                    _context.Update(account);
                    await _context.SaveChangesAsync();
                    return Results.Ok("Updated");
                }
            }

            return Results.Problem();
        }

    }
}
