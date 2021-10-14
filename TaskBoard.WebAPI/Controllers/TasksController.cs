using System;
using System.Linq;
using System.Security.Claims;
using System.Collections.Generic;

using TaskBoard.Data;
using TaskBoard.WebAPI.Models.Response;
using TaskBoard.WebAPI.Models.Task;
using TaskBoard.WebAPI.Models.User;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace TaskBoard.WebAPI.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/tasks")]
    public class TasksController : Controller
    {
        private readonly ApplicationDbContext dbContext;

        public TasksController(ApplicationDbContext context)
        {
            this.dbContext = context;
        }

        /// <summary>
        /// Gets tasks count.
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     GET /api/tasks/count
        ///     {
        ///         
        ///     }
        /// </remarks>
        /// <response code="200">Returns "OK" with tasks count</response>  
        [HttpGet("count")]
        [AllowAnonymous]
        public IActionResult GetTasksCount()
        {
            var tasks = new List<TaskCountListingModel>();

            tasks.Add(new TaskCountListingModel()
            {
                BoardName = "All",
                TasksCount = this.dbContext.Tasks.Count()
            });

            var taskBoards = this.dbContext
                .Boards
                .Select(b => new TaskCountListingModel()
                {
                    BoardName = b.Name,
                    TasksCount = b.Tasks.Count()
                });
            tasks.AddRange(taskBoards);

            return Ok(tasks);
        }

        /// <summary>
        /// Gets a list with all tasks.
        /// </summary>
        /// <remarks>
        /// You should be an authenticated user!
        /// 
        /// Sample request:
        ///
        ///     GET /api/tasks
        ///     {
        ///         
        ///     }
        /// </remarks>
        /// <response code="200">Returns "OK" with a list of all tasks</response>
        /// <response code="401">Returns "Unauthorized" when user is not authenticated</response>    
        [HttpGet()]
        public IActionResult GetTasks()
        {
            var tasks = this.dbContext
                .Tasks
                .Select(t => new TaskExtendedListingModel()
                {
                    Id = t.Id,
                    Title = t.Title,
                    Description = t.Description,
                    CreatedOn = t.CreatedOn.ToString("dd/MM/yyyy HH:mm"),
                    Board = t.Board.Name,
                    Owner = new UserListingModel()
                    {
                        Id = t.OwnerId,
                        Username = t.Owner.UserName,
                        FirstName = t.Owner.FirstName,
                        LastName = t.Owner.LastName,
                        Email = t.Owner.Email
                    }
                })
                .ToList();

            return Ok(tasks);
        }

        /// <summary>
        /// Gets a task by id.
        /// </summary>
        /// <remarks>
        /// You should be an authenticated user!
        /// 
        /// Sample request:
        ///
        ///     GET /api/tasks/{id}
        ///     {
        ///         
        ///     }
        /// </remarks>
        /// <response code="200">Returns "OK" with the task</response>
        /// <response code="401">Returns "Unauthorized" when user is not authenticated</response>    
        /// <response code="404">Returns "Not Found" when task with the given id doesn't exist</response> 
        [HttpGet("{id}")]
        public IActionResult GetTaskById(int id)
        {
            var taskExists = this.dbContext.Tasks.Any(t => t.Id == id);
            if (!taskExists)
            {
                return NotFound(
                    new ResponseMsg { Message = $"Task #{id} not found." });
            }

            var task = CreateTaskListingModelById(id);
            return Ok(task);
        }

        /// <summary>
        /// Gets tasks by keyword.
        /// </summary>
        /// <remarks>
        /// You should be an authenticated user!
        /// 
        /// Sample request:
        ///
        ///     GET /api/tasks/search/{keyword}
        ///     {
        ///         
        ///     }
        /// </remarks>
        /// <response code="200">Returns "OK" with the tasks</response>
        /// <response code="401">Returns "Unauthorized" when user is not authenticated</response>
        [HttpGet("search/{keyword}")]
        public IActionResult GetTasksByKeyword(string keyword)
        {
            var tasks = this.dbContext
                .Tasks
                .Where(t => t.Title.Contains(keyword) || t.Description.Contains(keyword))
                .Select(t => new TaskExtendedListingModel()
                {
                    Id = t.Id,
                    Title = t.Title,
                    Description = t.Description,
                    CreatedOn = t.CreatedOn.ToString("dd/MM/yyyy HH:mm"),
                    Board = t.Board.Name,
                    Owner = new UserListingModel()
                    {
                        Id = t.OwnerId,
                        Username = t.Owner.UserName,
                        FirstName = t.Owner.FirstName,
                        LastName = t.Owner.LastName,
                        Email = t.Owner.Email
                    }
                })
                .ToList();

            return Ok(tasks);
        }

        /// <summary>
        /// Gets tasks by board name.
        /// </summary>
        /// <remarks>
        /// You should be an authenticated user!
        /// 
        /// Sample request:
        ///
        ///     GET /api/tasks/board/{boardName}
        ///     {
        ///         
        ///     }
        /// </remarks>
        /// <response code="200">Returns "OK" with the tasks</response>
        /// <response code="401">Returns "Unauthorized" when user is not authenticated</response>
        [HttpGet("board/{boardName}")]
        public IActionResult GetTasksByBoardName(string boardName)
        {
            var tasks = this.dbContext
                .Tasks
                .Where(t => t.Board.Name == boardName)
                .Select(t => new TaskExtendedListingModel()
                {
                    Id = t.Id,
                    Title = t.Title,
                    Description = t.Description,
                    CreatedOn = t.CreatedOn.ToString("dd/MM/yyyy HH:mm"),
                    Board = t.Board.Name,
                    Owner = new UserListingModel()
                    {
                        Id = t.OwnerId,
                        Username = t.Owner.UserName,
                        FirstName = t.Owner.FirstName,
                        LastName = t.Owner.LastName,
                        Email = t.Owner.Email
                    }
                })
                .ToList();

            return Ok(tasks);
        }

        /// <summary>
        /// Creates a new task.
        /// </summary>
        /// <remarks>
        /// You should be an authenticated user!
        /// 
        /// Sample request:
        ///
        ///     POST /api/tasks/create
        ///     {
        ///            "title": "Add Tests",
        ///            "description": "API + UI tests",
        ///            "board": "Open"
        ///     }
        /// </remarks>
        /// <response code="201">Returns "Created" with the created event</response>
        /// <response code="400">Returns "Bad Request" when an invalid request is sent</response>   
        /// <response code="401">Returns "Unauthorized" when user is not authenticated</response> 
        [HttpPost("create")]
        public IActionResult CreateTask(TaskBindingModel taskModel)
        {
            if (!this.dbContext.Boards.Any(b => b.Name == taskModel.Board))
            {
                return BadRequest(
                    new ResponseMsg { Message = $"Board {taskModel.Board} does not exist." });
            }

            Task task = new Task()
            {
                Title = taskModel.Title,
                Description = taskModel.Description,
                BoardId = GetBoardId(taskModel.Board),
                CreatedOn = DateTime.Now,
                OwnerId = GetCurrentUserId()
            };

            this.dbContext.Tasks.Add(task);
            this.dbContext.SaveChanges();

            var taskListingModel = CreateTaskListingModelById(task.Id);

            return CreatedAtAction("GetTaskById", new { id = task.Id }, taskListingModel);
        }

        /// <summary>
        /// Edits a task.
        /// </summary>
        /// <remarks>
        /// You should be an authenticated user!
        /// You should be the owner of the edited task!
        /// 
        /// Sample request:
        ///
        ///     PUT /api/tasks/{id}
        ///     {
        ///            "title": "Add More Tests",
        ///            "description": "API + UI tests",
        ///            "board": "In Progress"
        ///     }
        /// </remarks>
        /// <response code="204">Returns "No Content"</response>
        /// <response code="400">Returns "Bad Request" when an invalid request is sent</response>   
        /// <response code="401">Returns "Unauthorized" when user is not authenticated or is not the owner of the task</response>  
        /// <response code="404">Returns "Not Found" when task with the given id doesn't exist</response>  
        [HttpPut("{id}")]
        public IActionResult PutTask(int id, TaskBindingModel taskModel)
        {
            var taskExists = this.dbContext.Tasks.Any(t => t.Id == id);
            if (!taskExists)
            {
                return NotFound(
                    new ResponseMsg { Message = $"Task #{id} not found." });
            }

            var task = this.dbContext.Tasks.FirstOrDefault(t => t.Id == id);

            if (GetCurrentUserId() != task.OwnerId)
            {
                return Unauthorized(
                    new ResponseMsg { Message = "Cannot edit task, when not an owner." });
            }

            if (!this.dbContext.Boards.Any(b => b.Name == taskModel.Board))
            {
                return BadRequest(
                    new ResponseMsg { Message = $"Board {taskModel.Board} does not exist." });
            }

            task.Title = taskModel.Title;
            task.Description = taskModel.Description;
            task.BoardId = GetBoardId(taskModel.Board);

            this.dbContext.SaveChanges();

            return NoContent();
        }


        /// <summary>
        /// Partially edits a task.
        /// </summary>
        /// <remarks>
        /// You should be an authenticated user!
        /// You should be the owner of the edited task!
        /// 
        /// Sample request:
        ///
        ///     PATCH /api/tasks/{id}
        ///     {
        ///            "title": "Add Even More Tests",
        ///     }
        /// </remarks>
        /// <response code="204">Returns "No Content"</response>
        /// <response code="400">Returns "Bad Request" when an invalid request is sent</response>   
        /// <response code="401">Returns "Unauthorized" when user is not authenticated or is not the owner of the task</response>  
        /// <response code="404">Returns "Not Found" when task with the given id doesn't exist</response>
        [HttpPatch("{id}")]
        public IActionResult PatchTask(int id, TaskPatchModel taskModel)
        {
            var taskExists = this.dbContext.Tasks.Any(t => t.Id == id);
            if (!taskExists)
            {
                return NotFound(
                    new ResponseMsg { Message = $"Task #{id} not found." });
            }

            var task = this.dbContext.Tasks.FirstOrDefault(t => t.Id == id);

            if (GetCurrentUserId() != task.OwnerId)
            {
                return Unauthorized(
                    new ResponseMsg { Message = "Cannot edit task, when not an owner." });
            }

            task.Title = string.IsNullOrEmpty(taskModel.Title) ? task.Title : taskModel.Title;
            task.Description = string.IsNullOrEmpty(taskModel.Description) ? task.Description : taskModel.Description;

            if (!string.IsNullOrEmpty(taskModel.Board))
            {
                if (!this.dbContext.Boards.Any(b => b.Name == taskModel.Board))
                {
                    return BadRequest(
                        new ResponseMsg { Message = $"Board {taskModel.Board} does not exist." });
                }
                task.BoardId = GetBoardId(taskModel.Board);
            }

            this.dbContext.SaveChanges();

            return NoContent();
        }

        /// <summary>
        /// Deletes a task.
        /// </summary>
        /// <remarks>
        /// You should be an authenticated user!
        /// You should be the owner of the deleted task!
        /// 
        /// Sample request:
        ///
        ///     DELETE /api/tasks/{id}
        ///     {
        ///            
        ///     }
        /// </remarks>
        /// <response code="200">Returns "OK" with the deleted task</response>
        /// <response code="401">Returns "Unauthorized" when user is not authenticated or is not the owner of the task</response>  
        /// <response code="404">Returns "Not Found" when task with the given id doesn't exist</response> 
        [HttpDelete("{id}")]
        public IActionResult DeleteTask(int id)
        {
            var taskExists = this.dbContext.Tasks.Any(t => t.Id == id);
            if (!taskExists)
            {
                return NotFound(
                    new ResponseMsg { Message = $"Task #{id} not found." });
            }

            var task = this.dbContext.Tasks.FirstOrDefault(t => t.Id == id);

            if (GetCurrentUserId() != task.OwnerId)
            {
                return Unauthorized(
                    new ResponseMsg { Message = "Cannot delete task, when not an owner." });
            }

            var taskListingModel = CreateTaskListingModelById(task.Id);

            this.dbContext.Tasks.Remove(task);
            this.dbContext.SaveChanges();

            return Ok(taskListingModel);
        }

        private TaskExtendedListingModel CreateTaskListingModelById(int id)
            => this.dbContext
                .Tasks
                .Where(t => t.Id == id)
                .Select(t => 
                    new TaskExtendedListingModel()
                    {
                        Id = t.Id,
                        Title = t.Title,
                        Description = t.Description,
                        CreatedOn = t.CreatedOn.ToString("dd/MM/yyyy HH:mm"),
                        Board = t.Board.Name,
                        Owner = new UserListingModel()
                        {
                            Id = t.OwnerId,
                            Username = t.Owner.UserName,
                            FirstName = t.Owner.FirstName,
                            LastName = t.Owner.LastName,
                            Email = t.Owner.Email
                        }
                    })
                .FirstOrDefault();

        private string GetCurrentUserId()
        {
            string currentUsername = this.User.FindFirst(ClaimTypes.Name)?.Value;
            var currentUserId = this.dbContext
                .Users
                .FirstOrDefault(x => x.UserName == currentUsername)
                .Id;
            return currentUserId;
        }

        private int GetBoardId(string boardName)
            => this.dbContext.Boards.FirstOrDefault(b => b.Name == boardName).Id;
    }
}
