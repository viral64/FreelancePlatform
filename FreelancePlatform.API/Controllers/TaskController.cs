using FreelancePlatform.Application.Tasks;
using FreelancePlatform.Application.Tasks.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Collections.Generic; // Required for IEnumerable

namespace FreelancePlatform.API.Controllers
{
    [Route("api/tasks")] // Corrected route to be "api/tasks"
    [ApiController]
    [Authorize] // Ensures only authenticated users can access endpoints in this controller
    public class TaskController : ControllerBase
    {
        private readonly ITaskService _taskService;

        public TaskController(ITaskService taskService)
        {
            _taskService = taskService;
        }

        private int GetCurrentUserId()
        {
            // Helper method to get the current user's ID from claims
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out var userId))
            {
                // This should not happen if [Authorize] attribute is working correctly
                // and token contains NameIdentifier claim.
                // Consider how to handle this error case, perhaps throw an exception
                // or return an appropriate error response from the action method.
                // For now, throwing an exception as it indicates a fundamental issue.
                throw new InvalidOperationException("User ID not found in token or is invalid.");
            }
            return userId;
        }

        // POST: api/tasks
        [HttpPost]
        public async Task<IActionResult> CreateTask([FromBody] CreateTaskDto createTaskDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var clientId = GetCurrentUserId();
                var taskDto = await _taskService.CreateTaskAsync(createTaskDto, clientId);
                // Return 201 Created with the location of the new resource and the resource itself
                return CreatedAtAction(nameof(GetTaskById), new { id = taskDto.TaskId }, taskDto);
            }
            catch (InvalidOperationException ex) // Catch specific exception from GetCurrentUserId
            {
                return Unauthorized(new { message = ex.Message }); // Or BadRequest
            }
            catch (Exception ex) // Generic error handler
            {
                // Log the exception (not shown here)
                return StatusCode(500, "An unexpected error occurred while creating the task.");
            }
        }

        // GET: api/tasks/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetTaskById(int id)
        {
            try
            {
                var clientId = GetCurrentUserId();
                var taskDto = await _taskService.GetTaskByIdAsync(id, clientId);

                if (taskDto == null)
                {
                    // This means either the task doesn't exist or the user is not authorized to see it.
                    // Returning NotFound for both cases is common to avoid leaking information.
                    return NotFound(new { message = $"Task with ID {id} not found or access denied." });
                }

                return Ok(taskDto);
            }
            catch (InvalidOperationException ex) // Catch specific exception from GetCurrentUserId
            {
                 return Unauthorized(new { message = ex.Message }); // Or BadRequest
            }
            catch (Exception ex) // Generic error handler
            {
                // Log the exception (not shown here)
                return StatusCode(500, "An unexpected error occurred while retrieving the task.");
            }
        }

        // GET: api/tasks
        [HttpGet]
        public async Task<IActionResult> GetClientTasks()
        {
            try
            {
                var clientId = GetCurrentUserId();
                var tasks = await _taskService.GetTasksByClientIdAsync(clientId);
                return Ok(tasks);
            }
            catch (InvalidOperationException ex) // Catch specific exception from GetCurrentUserId
            {
                 return Unauthorized(new { message = ex.Message }); // Or BadRequest
            }
            catch (Exception ex) // Generic error handler
            {
                // Log the exception (not shown here)
                return StatusCode(500, "An unexpected error occurred while retrieving tasks.");
            }
        }
    }
}
