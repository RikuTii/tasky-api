
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
using System.Threading.Tasks;

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
                    Include(e => e.Creator).
                    Include(e => e.TaskListMetas).
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
                    ThenInclude(e => e.Creator).
                    Include(e => e.UserAccount).
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
                if (taskLists.Count > 0)
                {
                    List<TaskyAPI.Models.Task> retnTasks = new List<TaskyAPI.Models.Task>();
                    foreach (var tasklist in taskLists)
                    {
                        var tasksOnList = tasklist.Tasks;
                        if (tasksOnList != null)
                        {
                            foreach (var task in tasksOnList)
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



        [HttpGet("GetTaskList/{taskListId}")]
        public async Task<IResult> TaskList(int taskListId)
        {
            if (taskListId > 0)
            {
                TaskList? tasklist = await _context.TaskList.Where(e => e.Id == taskListId)
                    .Include(e => e.Tasks!.OrderBy(e => e.Ordering))
                    .ThenInclude(e => e.Meta)
                    .ThenInclude(e => e.File).FirstOrDefaultAsync();
                if (tasklist != null)
                {
                    bool ok = await IsAuthorizedToTaskList(tasklist);
                    if (ok)
                    {
                        return Results.Ok(tasklist);
                    }
                }
            }

            return Results.Unauthorized();
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

        [HttpPost("UpdateTaskList")]
        public async Task<IResult> UpdateTaskList([FromBody] TaskyAPI.Models.TaskList tasklist)
        {
            var accountId = HttpContext.Items["account_id"];
            if (accountId != null)
            {
                int account_id = (int)accountId;

                UserAccount account = await _context.UserAccount.Where(e => e.Id == account_id).FirstAsync();
                if (account != null)
                {

                    TaskList? updateList = await _context.TaskList.Where(e => e.Id == tasklist.Id).FirstOrDefaultAsync();
                    if (updateList != null)
                    {
                        bool ok = await IsAuthorizedToTaskList(updateList);
                        if (ok)
                        {
                            updateList.Name = tasklist.Name;
                            updateList.Description = tasklist.Description;
                            _context.Update(updateList);
                            await _context.SaveChangesAsync();
                            return Results.Ok();
                        }
                    }


                }
            }

            return Results.BadRequest();

        }

        [HttpPost("CreateTaskList")]
        public async Task<IResult> CreateTaskList([FromBody] TaskyAPI.Models.TaskList tasklist)
        {
            var accountId = HttpContext.Items["account_id"];
            if (accountId != null)
            {
                int account_id = (int)accountId;

                UserAccount account = await _context.UserAccount.Where(e => e.Id == account_id).FirstAsync();
                if (account != null)
                {
                    tasklist.CreatedDate = DateTime.Now;
                    tasklist.CreatorId = account.Id;
                    tasklist.Creator = account;
                    _context.Add(tasklist);
                    await _context.SaveChangesAsync();

                    return Results.Ok("OK");
                }
            }

            return Results.BadRequest();
        }

        [HttpPost("CreateTaskListWithTasks")]
        public async Task<IResult> CreateTaskListWithTasks([FromBody] JsonValue json)
        {
            JObject data = JObject.Parse(json.ToString());
            var tasks = data["tasks"]?.ToString();
            if (tasks == null) return Results.Problem();
            var accountId = HttpContext.Items["account_id"];
            if (accountId != null)
            {
                int account_id = (int)accountId;

                UserAccount account = await _context.UserAccount.Where(e => e.Id == account_id).FirstAsync();
                if (account != null)
                {
                    TaskList tasklist = new()
                    {
                        Name = data["Name"].ToString(),
                        Description = data["Description"].ToString(),
                        CreatedDate = DateTime.Now,
                        CreatorId = account.Id,
                    };
                    _context.Add(tasklist);
                    await _context.SaveChangesAsync();

                    char[] separators = new char[] { ';', ',', '\n' };
                    IEnumerable<string> result = tasks.Split(separators).Take(30);
                    for (int index = 0;index < result.Count(); index++) 
                    {
                        Models.Task newTask = new()
                        {
                            TaskListId = tasklist.Id,
                            Title = result.ElementAt(index),
                            CreatorId = account.Id,
                            CreatedDate = DateTime.Now,
                            Status = (int)TaskyStatus.NotDone,
                            Ordering = index + 1
                        };

                        _context.Add(newTask);
                        await _context.SaveChangesAsync();
                    }

                    return Results.Ok();
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

                            Notification newNotification = new()
                            {
                                Name = tasklist.Name + " tasklist has been shared to you",
                                Data = "",
                                ReceiverId = (int)account.Id,
                                CreatedDate = DateTime.Now
                            };

                            _context.Add(newNotification);
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
                            if (meta != null)
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
