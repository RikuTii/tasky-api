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

        [HttpPost("ReOrderTasks")]
        public void ReOrderTasks([FromBody] JsonValue json)
        {
            JObject data = JObject.Parse(json.ToString());
            Int32.TryParse(data["taskListId"]?.ToString(), out int taskListId);
            if (taskListId == 0) return;
            var tasks = data["tasks"]?.ToList();
            if (tasks == null) return;

            for (var index = 0; index < tasks.Count; index++)
            {
                var as_task = tasks[index].ToObject<TaskyAPI.Models.Task>();
                if (as_task == null) continue;
                if (as_task.Id < 1) continue;

                var taskList = _context.TaskList.Where(e => e.Id == taskListId).Include(e => e.Tasks)?.First();

                var task = taskList?.Tasks?.Where(e => e.Id == as_task.Id).First();
                if (task != null)
                {
                    task.Ordering = index + 1;
                    _context.Update(task);
                    _context.SaveChanges();
                }
            }
        }

        [HttpPost("RemoveTask")]
        public void RemoveTask([FromBody] JsonValue json)
        {
            JObject data = JObject.Parse(json.ToString());
            var idString = data["id"]?.ToString();
            Int32.TryParse(idString, out int taskId);
            Int32.TryParse(data["taskListId"]?.ToString(), out int taskListId);
            var accountId = HttpContext.Items["account_id"];

            var taskList = _context.TaskList.Where(e => e.Id == taskListId).Include(e => e.Tasks).First();
            if (taskList != null && accountId != null)
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                UserAccount authUser = _context.UserAccount.Where(e => e.Id == (int)accountId).First();
                if (authUser.Id == taskList.CreatorId)
                {
                    var task = taskList.Tasks?.Where(e => e.Id == taskId).First();
                    if (task != null)
                    {
                        _context.Remove(task);
                        _context.SaveChanges();

                        var tasks = taskList.Tasks?.ToList();

                        for (var index = 0; index < tasks.Count; index++)
                        {
                            if (task.Id == tasks[index].Id)
                                continue;
                            var orderTask = tasks[index];
                            if (orderTask != null)
                            {
                                orderTask.Ordering = index + 1;
                                _context.Update(orderTask);
                                _context.SaveChanges();
                            }
                        }
                    }
                }
            }

        }
        [HttpPost("CreateOrUpdateTask")]
        public void CreateOrUpdateTask([FromBody] JsonValue json)
        {
            JObject data = JObject.Parse(json.ToString());
            var idString = data["id"]?.ToString();

            Int32.TryParse(idString, out int taskId);
            Int32.TryParse(data["taskListId"]?.ToString(), out int taskListId);
            Int32.TryParse(data["status"]?.ToString(), out int statusOut);
            var accountId = HttpContext.Items["account_id"];
            if(accountId == null)
            {
                return;
            }

            if (((TaskyStatus)statusOut) == TaskyStatus.NotCreated)
            {
                var newTask = new TaskyAPI.Models.Task();
                newTask.TaskListId = taskListId;
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                UserAccount authUser = _context.UserAccount.Where(e => e.Id == (int)accountId).First();
                if (authUser != null)
                {
                    newTask.CreatorId = authUser.Id;
                }
                newTask.Title = data["title"]?.ToString();
                newTask.CreatedDate = DateTime.Now;
                newTask.Status = (int)TaskyStatus.NotDone;
                var numTasks = _context.Task.Where(e => e.TaskListId == taskListId).Count();
                newTask.Ordering = numTasks;
                _context.Add(newTask);
                _context.SaveChanges();
            }
            else
            {
                var taskQuery = _context.Task.Where(e => e.Id == taskId).ToList();
                if(taskQuery.Count > 0)
                {
                    var task = taskQuery.ElementAt(0);
                    task.Title = data["title"]?.ToString();
                    task.Status = (int)statusOut;
                    _context.Update(task);
                    _context.SaveChanges();
                }
              
            }

        }
    }
}
