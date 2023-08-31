
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskyAPI.Data;
using TaskyAPI.Models;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Primitives;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using TaskyAPI.Middleware;

namespace TaskyAPI.Controllers
{
    [Authorize]
    [ServiceFilter(typeof(AuthTokenParseFilter))]
    [Route("[controller]")]
    public class TasklistController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TasklistController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("Index")]
        // GET: Tasks
        public string Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            JwtSecurityToken token = null;
            if (Request.Headers.Keys.Contains("Authorization"))
            {
                StringValues values;

                if (Request.Headers.TryGetValue("Authorization", out values))
                {
                    var jwt = values.ToString();

                    if (jwt.Contains("Bearer"))
                    {
                        jwt = jwt.Replace("Bearer", "").Trim();
                    }

                    var handler = new JwtSecurityTokenHandler();

                    token = handler.ReadJwtToken(jwt);


                    if (token == null)
                        return "";

                    userId = token.Subject.ToString();
                }
            }

            User user = _context.User.Where(e => e.Email == userId).Include(e => e.Account).First();
            if (user != null)
            {
                var tasklist = _context.TaskList.
                    Where(e => e.CreatorId == user.Account.Id).
                    Include(e => e.Tasks!.OrderBy(e => e.Ordering)).
                    ThenInclude(e => e.Creator).
                    Include(e => e.Creator).
                    Include(e => e.TaskListMetas!).
                    ThenInclude(e => e.UserAccount).
                    ToList();


                //clean list owner sensitive data
                var cleanTaskLists = new List<TaskList>();
                foreach (var task in tasklist)
                {
                    var list = new TaskList();

                    list = task;
                    if (list.TaskListMetas != null && !list.TaskListMetas.IsNullOrEmpty())
                    {
                        foreach (var meta in list.TaskListMetas)
                        {
                            meta.UserAccount.Avatar = "";
                            meta.UserAccount.Locale = "";
                            meta.UserAccount.UserId = 0;
                        }
                    }

                    cleanTaskLists.Add(list);
                }

                var extraLists = _context.TaskListMeta.
                    Where(e => e.UserAccountId == user.UserAccountId).
                    Include(e => e.TaskList).
                    ThenInclude(e => e.Creator).
                    Include(e => e.TaskList).
                    ThenInclude(e => e.Tasks!).
                    ThenInclude(e => e.Creator).
                    ToList();
                var extraTasks = new List<TaskList>();
                if (!extraLists.IsNullOrEmpty())
                {
                    foreach (var v in extraLists)
                    {
                        extraTasks.Add(v.TaskList);
                    }
                }

                var tasks = new List<TaskList>();

                tasks.AddRange(cleanTaskLists);
                tasks.AddRange(extraTasks);


                return JsonSerializer.Serialize(tasks, new JsonSerializerOptions
                {
                    ReferenceHandler = ReferenceHandler.IgnoreCycles,
                    DictionaryKeyPolicy = JsonNamingPolicy.CamelCase,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    WriteIndented = true
                });
            }

            return "";
        }

        bool IsAuthorizedToTaskList(TaskList list)
        {
            if (list != null)
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                User user = _context.User.Where(e => e.Email == userId).Include(e => e.Account).First();
                if (user != null)
                {
                    if (list.CreatorId == user.Account.Id)
                    {
                        return true;
                    }

                    var metas = _context.TaskListMeta.Where(e => e.TaskListId == list.Id).ToList();
                    foreach (var item in metas)
                    {
                        if (item.UserAccountId == user.Account.Id)
                        {
                            return true;
                        }
                    }


                }
            }
            return false;
        }

        [HttpGet("TaskList")]
        public IResult TaskList([FromQuery] int taskListId)
        {
            Console.WriteLine(taskListId);
            if (taskListId > 0)
            {
                var tasklist = _context.TaskList.Where(e => e.Id == taskListId).Include(e => e.Tasks!.OrderBy(e => e.Ordering)).First();
                if (tasklist != null)
                {
                    if (IsAuthorizedToTaskList(tasklist))
                    {
                        return Results.Ok(tasklist);
                    }
                }
            }

            return Results.BadRequest();
        }

        [HttpPost("CreateTaskList")]
        public async void CreateTaskList([FromBody] TaskyAPI.Models.TaskList task)
        {
            var userId = HttpContext.Items["user_id"];
            if (userId != null)
            {
                int user_id = (int)userId;
                User user = await _context.User.Where(e => e.Id == user_id).FirstAsync();
                if (user != null)
                {
                    UserAccount account = await _context.UserAccount.Where(e => e.UserId == user_id).FirstAsync();
                    if(account != null)
                    {
                        task.CreatedDate = DateTime.Now;
                        task.CreatorId = user.Account.Id;
                        task.Creator = user.Account;
                        _context.Add(task);
                        await _context.SaveChangesAsync();
                    }
      
                }
            }

        }
      /*  [HttpPost("ShareTaskList")]
        public void ShareTaskList([FromBody] JsonValue json)
        {
            JObject data = JObject.Parse(json.ToString());
            var idString = data["id"]?.ToString();
            if (Int32.TryParse(idString, out int id))
            {
                var email = data["email"]?.ToString();
                UserAccount account = _context.UserAccount.Where(e => e.Email == email).First();
                if (account != null)
                {
                    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                    ApplicationUser authUser = _context.Users.Where(e => e.Id == userId).Include(e => e.Account).First();
                    if (authUser != null)
                    {
                        //make sure caller owns the tasklist
                        TaskList tasklist = _context.TaskList.Where(e => e.Id == id).First();
                        if (tasklist.CreatorID == authUser.Account.Id)
                        {
                            TaskListMeta meta = new()
                            {
                                TaskListID = id,
                                UserAccountID = account.Id,
                                Id = null
                            };
                            _context.Add(meta);
                            _context.SaveChanges();
                        }
                    }
                }
            }
        }
        [HttpPost("RemoveShareTaskList")]
        public void RemoveShareTaskList([FromBody] JsonValue json)
        {
            JObject data = JObject.Parse(json.ToString());
            var idString = data["id"]?.ToString();
            if (Int32.TryParse(idString, out int id))
            {
                var email = data["email"]?.ToString();
                UserAccount account = _context.UserAccount.Where(e => e.Email == email).First();
                if (account != null)
                {
                    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                    ApplicationUser authUser = _context.Users.Where(e => e.Id == userId).Include(e => e.Account).First();
                    if (authUser != null)
                    {
                        //make sure caller owns the tasklist
                        TaskList tasklist = _context.TaskList.Where(e => e.Id == id).First();
                        if (tasklist.CreatorID == authUser.Account.Id)
                        {
                            _context.Remove(_context.TaskListMeta.Where(e => e.TaskListID == id).Where(e => e.UserAccountID == account.Id).First());
                            _context.SaveChanges();
                        }
                    }
                }
            }
        }*/
    }
}
