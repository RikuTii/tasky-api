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
    public class NotificationController : Controller
    {
        private readonly ApplicationDbContext _context;



        public NotificationController(ApplicationDbContext context)
        {
            _context = context;
        }


        [HttpGet("PollNotifications")]
        public async Task<IResult> PollNotifications()
        {
            var accountId = HttpContext.Items["account_id"];
            if (accountId != null)
            {
                int acccount_id = (int)accountId;

                List<Notification> notif = await _context.Notification.Where(e => e.ReceiverId == acccount_id).ToListAsync();
                if (notif != null)
                {
                    return Results.Ok(notif);
                }

            }

            return Results.NotFound();
        }


        [HttpPost("RemoveNotification")]
        public void RemoveNotification([FromBody] Notification deleteNotif)
        {
            var accountId = HttpContext.Items["account_id"];
            if (accountId != null)
            {
                int acccount_id = (int)accountId;

                Notification notif = _context.Notification.Where(e => e.Id == deleteNotif.Id).First();
                if (notif != null)
                {
                    _context.Remove(notif);
                    _context.SaveChangesAsync();
                }

            }
        }
    }
}
