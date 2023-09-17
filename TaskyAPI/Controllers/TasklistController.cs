
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
using System.Linq;
using System.Collections.Generic;

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
        public async Task<IResult> Index()
        {
            var accountId = HttpContext.Items["account_id"];

            if (accountId == null)
            {
                return Results.Unauthorized();
            }

            UserAccount? account = await _context.UserAccount.Where(e => e.Id == (int)accountId).FirstOrDefaultAsync();
            if (account != null)
            {
                var tasklist = await _context.TaskList.
                    Where(e => e.CreatorId == account.Id).
                    Include(e => e.Tasks!.OrderBy(e => e.Ordering)).
                    ThenInclude(e => e.Creator).
                    Include(e => e.Tasks!.OrderBy(e => e.Ordering)).
                    ThenInclude(e => e.Meta!).
                    ThenInclude(e => e.File).
                    Include(e => e.Creator).
                    Include(e => e.TaskListMetas!).
                    ThenInclude(e => e.UserAccount).
                    AsSplitQuery().
                    ToListAsync();


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

                var extraLists = await _context.TaskListMeta.
                    Where(e => e.UserAccountId == account.Id).
                    Include(e => e.TaskList).
                    ThenInclude(e => e.Tasks!.OrderBy(e => e.Ordering)).
                    ThenInclude(e => e.Creator).
                    Include(e => e.TaskList).
                    ThenInclude(e => e.Tasks!.OrderBy(e => e.Ordering)).
                    ThenInclude(e => e.Meta!).
                    ThenInclude(e => e.File).
                    Include(e => e.TaskList).
                    ThenInclude(e => e.Creator).
                    AsSplitQuery().
                    ToListAsync();

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

                return Results.Ok(tasks);
            }

            return Results.Problem();
        }

        async Task<bool> IsAuthorizedToTaskList(TaskList list)
        {
            if (list != null)
            {
                var accountId = HttpContext.Items["account_id"];
                if (accountId != null)
                {
                    UserAccount? account = await _context.UserAccount.Where(e => e.Id == (int)accountId).FirstOrDefaultAsync();
                    if (account != null)
                    {
                        if (list.CreatorId == account.Id)
                        {
                            return true;
                        }

                        var metas = await _context.TaskListMeta.Where(e => e.TaskListId == list.Id).ToListAsync();
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



        [HttpGet("GetTask/{taskId}")]
        public async Task<IResult> GetTask(int taskId)
        {
            if (taskId > 0)
            {
                var task = await _context.Task.Where(e => e.Id == taskId).OrderBy(e => e.Ordering).Include(e => e.Meta!).ThenInclude(e => e.File).Include(e => e.TaskList).FirstOrDefaultAsync();
                if (task != null)
                {
                    bool ok = await IsAuthorizedToTaskList(task.TaskList);
                    if (ok)
                    {
                        return Results.Ok(task);
                    }
                }
            }

            return Results.BadRequest();
        }



        [HttpGet("GetUpcomingTasks")]
        public async Task<IResult> GetUpcomingTasks()
        {
            var accountId = HttpContext.Items["account_id"];
            if (accountId != null)
            {
                int account_id = (int)accountId;
                List<TaskList> taskLists = await _context.TaskList.Where(e => e.CreatorId == account_id)
                    .Include(e => e.Tasks!.OrderBy(e => e.ScheduleDate)
                    .Where(e => e.ScheduleDate != null && e.ScheduleDate > DateTime.UtcNow))
                    .ToListAsync();
                if(taskLists.Count > 0)
                {
                    List<TaskyAPI.Models.Task> retnTasks = new List<TaskyAPI.Models.Task>();
                    foreach(var tasklist in taskLists)
                    {
                        var tasksOnList = tasklist.Tasks;
                        if(tasksOnList != null)
                        {
                            foreach(var task in tasksOnList)
                            {
                                retnTasks.Add(task);
                            }
                        }
                    }
                    return Results.Ok(retnTasks.OrderBy(e => e.ScheduleDate).Take(10).ToList());
                }

            }

            return Results.BadRequest();
        }



        [HttpGet("TaskList")]
        public async Task<IResult> TaskList([FromQuery] int taskListId)
        {
            if (taskListId > 0)
            {
                TaskList? tasklist = await _context.TaskList.Where(e => e.Id == taskListId).Include(e => e.Tasks!.OrderBy(e => e.Ordering)).FirstOrDefaultAsync();
                if (tasklist != null)
                {
                    bool ok = await IsAuthorizedToTaskList(tasklist);
                    if (ok)
                    {
                        return Results.Ok(tasklist);
                    }
                }
            }

            return Results.BadRequest();
        }


        [HttpPost("Delete")]
        public async Task<IResult> Delete([FromBody] TaskList list)
        {
            var accountId = HttpContext.Items["account_id"];
            if (accountId != null)
            {
                int account_id = (int)accountId;

                UserAccount account = await _context.UserAccount.Where(e => e.Id == account_id).FirstAsync();
                if (account != null)
                {

                    TaskList? deleteList = await _context.TaskList.Where(e => e.Id == list.Id).FirstOrDefaultAsync();
                    if (deleteList != null)
                    {
                        bool ok = await IsAuthorizedToTaskList(deleteList);
                        if (ok)
                        {
                            _context.Remove(deleteList);
                            await _context.SaveChangesAsync();
                            return Results.Ok();
                        }
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
        public async Task<IResult> ShareTaskList([FromBody] JsonValue json)
        {
            JObject data = JObject.Parse(json.ToString());
            var idString = data["id"]?.ToString();

            var accountId = HttpContext.Items["account_id"];


            if (Int32.TryParse(idString, out int id) && accountId != null)
            {
                var email = data["email"]?.ToString();
                UserAccount? account = await _context.UserAccount.Where(e => e.Email == email).FirstOrDefaultAsync();
                if (account != null)
                {
                    int account_id = (int)accountId;

                    UserAccount? authUser = await _context.UserAccount.Where(e => e.Id == account_id).FirstOrDefaultAsync();
                    if (authUser != null)
                    {
                        //make sure caller owns the tasklist
                        TaskList? tasklist = await _context.TaskList.Where(e => e.Id == id).FirstOrDefaultAsync();
                        if (tasklist != null && tasklist.CreatorId == authUser.Id)
                        {
                            TaskListMeta meta = new()
                            {
                                TaskListId = id,
                                UserAccountId = account.Id,
                            };
                            _context.Add(meta);
                            await _context.SaveChangesAsync();
                            return Results.Ok();
                        }
                    }
                }
            }
            return Results.Problem();
        }
        [HttpPost("RemoveShareTaskList")]
        public async Task<IResult> RemoveShareTaskList([FromBody] JsonValue json)
        {
            JObject data = JObject.Parse(json.ToString());
            var accountId = HttpContext.Items["account_id"];

            var idString = data["id"]?.ToString();
            if (Int32.TryParse(idString, out int id) && accountId != null)
            {
                var email = data["email"]?.ToString();
                UserAccount? account = await _context.UserAccount.Where(e => e.Email == email).FirstOrDefaultAsync();
                if (account != null)
                {
                    int account_id = (int)accountId;

                    UserAccount? authUser = await _context.UserAccount.Where(e => e.Id == account_id).FirstOrDefaultAsync();
                    if (authUser != null)
                    {
                        //make sure caller owns the tasklist
                        TaskList? tasklist = await _context.TaskList.Where(e => e.Id == id).FirstOrDefaultAsync();
                        if (tasklist != null && tasklist.CreatorId == authUser.Id)
                        {
                            TaskListMeta? meta = await _context.TaskListMeta.Where(e => e.TaskListId == id).Where(e => e.UserAccountId == account.Id).FirstOrDefaultAsync();
                            if(meta != null)
                            {
                                _context.Remove(meta);
                                await _context.SaveChangesAsync();
                                return Results.Ok();
                            }
                        }
                    }
                }
            }

            return Results.Problem();
        }
    }
}
