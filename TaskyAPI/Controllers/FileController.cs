using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using System.Security.Claims;
using System.Text.Json.Nodes;
using TaskyAPI.Data;
using TaskyAPI.Middleware;
using TaskyAPI.Models;
using static System.Net.Mime.MediaTypeNames;

namespace TaskyAPI.Controllers
{
    [Route("[controller]")]
    public class FileController : Controller
    {
        private readonly ApplicationDbContext _context;

        public FileController(ApplicationDbContext context)
        {
            _context = context;
        }

        [Route("loadimage/{*file}")]
        public async Task<IActionResult> LoadImage(string file)
        {
            string finalFile = "/" + file;
            if (System.IO.File.Exists(finalFile) && finalFile.Contains("usr/share"))
            {
                return File(System.IO.File.OpenRead(finalFile), "application/octet-stream", Path.GetFileName(finalFile));
            }

            return NotFound();
        }

        [Authorize]
        [ServiceFilter(typeof(AuthTokenParseFilter))]
        [HttpPost("OnPostUploadAsync")]
        public async Task<IActionResult> OnPostUploadAsync([FromForm] List<IFormFile> files, [FromForm] int task_id)
        {
            var accountId = HttpContext.Items["account_id"];
            if (accountId != null)
            {
                int account_id = (int)accountId;

                Dictionary<string, string> allowedTypes = new()
                {
                    { "image/gif", ".gif" },
                    { "image/jpeg", ".jpg" },
                    { "image/png", ".png" },
                    { "application/json", ".json" },
                    { "application/pdf", ".pdf" },
                    { "text/plain", ".txt" }
                };

                long size = files.Sum(f => f.Length);
                string folderPath = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
                foreach (var formFile in files)
                {
                    if (formFile.Length > 0 && allowedTypes.Where(e => e.Key == formFile.ContentType).Count() > 0)
                    {
                        var userPath = folderPath + "/tasky/" + account_id.ToString();
                        if (!Directory.Exists(userPath))
                        {
                            Directory.CreateDirectory(userPath);
                        }

                        string fileExtension = allowedTypes.Where(e => e.Key == formFile.ContentType).First().Value;
                        var filePath = userPath + "/" + Path.GetRandomFileName() + fileExtension;

                        await Console.Out.WriteLineAsync(filePath);
                        using (var stream = System.IO.File.Create(filePath))
                        {
                            await formFile.CopyToAsync(stream);
                        }

                        string fileType = "image";
                        if (fileExtension == ".json" || fileExtension == ".pdf" || fileExtension == ".txt")
                        {
                            fileType = "other";
                        }
                        TaskyAPI.Models.File file = new()
                        {
                            CreatedDate = DateTime.Now,
                            Name = formFile.FileName,
                            Path = filePath,
                            Type = fileType,
                        };

                        _context.Add(file);
                        await _context.SaveChangesAsync();

                        if (task_id > 0)
                        {
                            TaskMeta meta = new()
                            {
                                FileId = file.Id,
                                TaskId = task_id,
                            };
                            _context.Add(meta);
                            await _context.SaveChangesAsync();
                        }
                    }


                    return Ok(new { count = files.Count, size });

                }
            }

            return Unauthorized();
        }
    }
}
