using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MySqlX.XDevAPI.Common;
using Newtonsoft.Json.Linq;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Json.Nodes;
using TaskyAPI.Data;
using TaskyAPI.Middleware;
using TaskyAPI.Models;
using static System.Net.Mime.MediaTypeNames;
using System.Drawing;
using System.IO;
using System.Transactions;

namespace TaskyAPI.Controllers
{
    [Authorize]
    [ServiceFilter(typeof(AuthTokenParseFilter))]
    [Route("[controller]")]
    public class TaskController : Controller
    {
        private readonly ApplicationDbContext _context;

        public enum TaskyStatus
        {
            NotCreated,
            NotDone,
            Done
        }

        public TaskController(ApplicationDbContext context)
        {
            _context = context;
        }


        async Task<bool> IsAuthorizedToEditTask(TaskyAPI.Models.Task task)
        {
            if (task != null)
            {
                var accountId = HttpContext.Items["account_id"];
                if (accountId != null)
                {
                    UserAccount? account = await _context.UserAccount.Where(e => e.Id == (int)accountId).FirstOrDefaultAsync();
                    if (account != null)
                    {
                        TaskList? list = await _context.TaskList.Where(e => e.Id == task.TaskListId).FirstOrDefaultAsync();
                        if (list == null)
                            return false;
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

        [HttpPost("ReOrderTasks")]
        public async Task<IResult> ReOrderTasks([FromBody] JsonValue json)
        {
            JObject data = JObject.Parse(json.ToString());
            Int32.TryParse(data["taskListId"]?.ToString(), out int taskListId);
            if (taskListId == 0) return Results.Problem();
            var tasks = data["tasks"]?.ToList();
            if (tasks == null) return Results.Problem();
            bool hasUpdate = false;
            for (var index = 0; index < tasks.Count; index++)
            {
                var as_task = tasks[index].ToObject<TaskyAPI.Models.Task>();
                if (as_task == null) continue;
                if (as_task.Id < 1) continue;

                TaskList? taskList = await _context.TaskList.Where(e => e.Id == taskListId).Include(e => e.Tasks).FirstOrDefaultAsync();

                TaskyAPI.Models.Task? task = taskList?.Tasks?.Where(e => e.Id == as_task.Id).FirstOrDefault();
                if (task != null && await IsAuthorizedToEditTask(task))
                {
                    task.Ordering = index + 1;
                    _context.Update(task);
                    hasUpdate = true;
                    await _context.SaveChangesAsync();
                }
            }


            if (hasUpdate)
            {
                return Results.Ok();
            }
            else
            {
                return Results.Problem();
            }
        }

        [HttpPost("RemoveTask")]
        public async Task<IResult> RemoveTask([FromBody] JsonValue json)
        {
            JObject data = JObject.Parse(json.ToString());
            var idString = data["id"]?.ToString();
            Int32.TryParse(idString, out int taskId);
            Int32.TryParse(data["taskListId"]?.ToString(), out int taskListId);
            var accountId = HttpContext.Items["account_id"];

            TaskList? taskList = await _context.TaskList.Where(e => e.Id == taskListId).Include(e => e.Tasks).FirstOrDefaultAsync();
            if (taskList != null && accountId != null)
            {

        
                    TaskyAPI.Models.Task? task = taskList.Tasks?.Where(e => e.Id == taskId).FirstOrDefault();
                    if (task != null && await IsAuthorizedToEditTask(task))
                    {
                        _context.Remove(task);
                        await _context.SaveChangesAsync();

                        var tasks = taskList.Tasks?.ToList();
                        if (tasks != null)
                        {
                            for (var index = 0; index < tasks.Count; index++)
                            {
                                if (task.Id == tasks[index].Id)
                                    continue;
                                var orderTask = tasks[index];
                                if (orderTask != null)
                                {
                                    orderTask.Ordering = index + 1;
                                    _context.Update(orderTask);
                                    await _context.SaveChangesAsync();
                                    return Results.Ok();
                                }
                            }
                        }

                    
                }
            }

            return Results.Problem();
        }


        [HttpPost("CreateOrUpdateTask")]
        public async Task<IResult> CreateOrUpdateTask([FromBody] TaskyAPI.Models.Task task)
        {
            var accountId = HttpContext.Items["account_id"];
            if (accountId == null)
            {
                return Results.Unauthorized();
            }

            bool canCreateOrEdit = await IsAuthorizedToEditTask(task);

            if(!canCreateOrEdit)
            {
                return Results.Unauthorized();
            }

            if (task.Status == (int)TaskyStatus.NotCreated)
            {
                TaskyAPI.Models.Task newTask = new();
                UserAccount? authUser = await _context.UserAccount.Where(e => e.Id == (int)accountId).FirstOrDefaultAsync();
                if (authUser != null)
                {
                    newTask.CreatorId = authUser.Id;
                }
                newTask.TaskListId = task.TaskListId;
                newTask.Title = task.Title;
                newTask.Description = task.Description;
                newTask.CreatedDate = DateTime.Now;
                newTask.Status = (int)TaskyStatus.NotDone;
                var numTasks = await _context.Task.Where(e => e.TaskListId == task.TaskListId).CountAsync();
                newTask.Ordering = numTasks;
                _context.Add(newTask);
                await _context.SaveChangesAsync();

                return Results.Ok(newTask);
            }
            else
            {
                var taskQuery = await _context.Task.Where(e => e.Id == task.Id).ToListAsync();
                if (taskQuery.Count > 0)
                {
                    var firstTask = taskQuery.ElementAt(0);
                    firstTask.Title = task.Title;
                    firstTask.Description = task.Description;
                    firstTask.Status = task.Status;
                    firstTask.TimeTrack = task.TimeTrack;
                    firstTask.TimeElapsed = task.TimeElapsed;
                    firstTask.TimeEstimate = task.TimeEstimate;
                    firstTask.ScheduleDate = task.ScheduleDate;

                    _context.Update(firstTask);
                    await _context.SaveChangesAsync();

                    TaskyAPI.Models.Task markTask = new()
                    {
                        Status = 0,
                    };

                    return Results.Ok(markTask);
                }
            }

            return Results.Ok();
        }


        [HttpPost("AddAttachment")]
        public async Task<IResult> AddAttachment([FromBody] JsonValue json)
        {
            JObject data = JObject.Parse(json.ToString());
            Int32.TryParse(data["taskId"]?.ToString(), out int taskId);
            if (taskId == 0) return Results.Problem();

            TaskyAPI.Models.File file = new TaskyAPI.Models.File
            {
                CreatedDate = DateTime.Now,
                Name = "Test file",
                Path = "www.google.com"
            };

            _context.Add(file);
            await _context.SaveChangesAsync();

            TaskMeta meta = new TaskMeta { TaskId = taskId, FileId = file.Id };
            _context.Add(meta);
            await _context.SaveChangesAsync();

            return Results.Ok();
        }

        [HttpPost("RemoveAttachment")]
        public async Task<IResult> RemoveAttachment([FromBody] TaskMeta meta)
        {
            TaskMeta? findMeta = await _context.TaskMeta.Where(e => e.Id == meta.Id).FirstOrDefaultAsync();
            if (findMeta != null)
            {
                _context.Remove(findMeta);
                await _context.SaveChangesAsync();
                return Results.Ok();
            }

            return Results.Unauthorized();
        }

    }
}
