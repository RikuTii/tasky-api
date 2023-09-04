
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskyAPI.Data;
using TaskyAPI.Models;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.IdentityModel.Tokens;
using TaskyAPI.Middleware;
using Microsoft.AspNetCore.Authorization;
using System.Text.Json.Nodes;
using Newtonsoft.Json.Linq;

namespace TaskyAPI.Controllers
{
    [Authorize]
    [ServiceFilter(typeof(AuthTokenParseFilter))]
    [Route("[controller]")]
    public class TasklistController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;


        public TasklistController(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [HttpGet("Index")]
        // GET: Tasks
        public string Index()
        {
            var accountId = HttpContext.Items["account_id"];

            if (accountId == null)
            {
                return "";
            }

            UserAccount account = _context.UserAccount.Where(e => e.Id == (int)accountId).First();
            if (account != null)
            {
                var tasklist = _context.TaskList.
                    Where(e => e.CreatorId == account.Id).
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
                    Where(e => e.UserAccountId == account.Id).
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
                var accountId = HttpContext.Items["account_id"];
                if (accountId != null)
                {
                    UserAccount account = _context.UserAccount.Where(e => e.Id == (int)accountId).First();
                    if (account != null)
                    {
                        if (list.CreatorId == account.Id)
                        {
                            return true;
                        }

                        var metas = _context.TaskListMeta.Where(e => e.TaskListId == list.Id).ToList();
                        foreach (var item in metas)
                        {
                            if (item.UserAccountId == account.Id)
                            {
                                return true;
                            }
                        }


                    }
                }

            }
            return false;
        }

        [HttpGet("TaskList")]
        public IResult TaskList([FromQuery] int taskListId)
        {
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
        public async Task<IResult> CreateTaskList([FromBody] TaskyAPI.Models.TaskList task)
        {
            var accountId = HttpContext.Items["account_id"];
            if (accountId != null)
            {
                int account_id = (int)accountId;

                UserAccount account = await _context.UserAccount.Where(e => e.Id == account_id).FirstAsync();
                if (account != null)
                {
                    task.CreatedDate = DateTime.Now;
                    task.CreatorId = account.Id;
                    task.Creator = account;
                    _context.Add(task);
                    await _context.SaveChangesAsync();

                    return Results.Ok("OK");
                }
            }

            return Results.BadRequest();

        }
        [HttpPost("ShareTaskList")]
        public void ShareTaskList([FromBody] JsonValue json)
        {
            JObject data = JObject.Parse(json.ToString());
            var idString = data["id"]?.ToString();

            var accountId = HttpContext.Items["account_id"];
        

            if (Int32.TryParse(idString, out int id) && accountId != null)
            {
                var email = data["email"]?.ToString();
                UserAccount account = _context.UserAccount.Where(e => e.Email == email).First();
                if (account != null)
                {
                    int account_id = (int)accountId;

                    UserAccount authUser = _context.UserAccount.Where(e => e.Id == account_id).First();
                    if (authUser != null)
                    {
                        //make sure caller owns the tasklist
                        TaskList tasklist = _context.TaskList.Where(e => e.Id == id).First();
                        if (tasklist.CreatorId == authUser.Id)
                        {
                            TaskListMeta meta = new()
                            {
                                TaskListId = id,
                                UserAccountId = account.Id,
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
            var accountId = HttpContext.Items["account_id"];

            var idString = data["id"]?.ToString();
            if (Int32.TryParse(idString, out int id) && accountId != null)
            {
                var email = data["email"]?.ToString();
                UserAccount account = _context.UserAccount.Where(e => e.Email == email).First();
                if (account != null)
                {
                    int account_id = (int)accountId;

                    UserAccount authUser = _context.UserAccount.Where(e => e.Id == account_id).First();
                    if (authUser != null)
                    {
                        //make sure caller owns the tasklist
                        TaskList tasklist = _context.TaskList.Where(e => e.Id == id).First();
                        if (tasklist.CreatorId == authUser.Id)
                        {
                            _context.Remove(_context.TaskListMeta.Where(e => e.TaskListId == id).Where(e => e.UserAccountId == account.Id).First());
                            _context.SaveChanges();
                        }
                    }
                }
            }
        }
    }
}
