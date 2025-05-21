using FreelancePlatform.Application.Tasks.Dtos;
using FreelancePlatform.Domain.Entities;
using FreelancePlatform.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System; // Required for DateTime

namespace FreelancePlatform.Application.Tasks
{
    public class TaskService : ITaskService
    {
        private readonly ViralDbContext _dbContext;

        public TaskService(ViralDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<TaskDto> CreateTaskAsync(CreateTaskDto taskDto, int clientId)
        {
            var task = new Task // Domain.Entities.Task
            {
                Title = taskDto.Title,
                Description = taskDto.Description,
                Budget = taskDto.Budget,
                Deadline = taskDto.Deadline,
                ClientId = clientId,
                Status = "Open", // Default status
                CreatedAt = DateTime.UtcNow
            };

            _dbContext.Tasks.Add(task);
            await _dbContext.SaveChangesAsync();

            return new TaskDto
            {
                TaskId = task.TaskId,
                ClientId = task.ClientId,
                Title = task.Title,
                Description = task.Description,
                Budget = task.Budget,
                Deadline = task.Deadline,
                Status = task.Status,
                CreatedAt = task.CreatedAt
            };
        }

        public async Task<TaskDto?> GetTaskByIdAsync(int taskId, int clientId)
        {
            var task = await _dbContext.Tasks
                .AsNoTracking() // Good practice for read-only queries
                .FirstOrDefaultAsync(t => t.TaskId == taskId);

            if (task == null)
            {
                return null; // Task not found
            }

            if (task.ClientId != clientId)
            {
                // Authorization check: Client can only see their own tasks
                // Depending on requirements, could throw an exception or just return null
                return null; 
            }

            return new TaskDto
            {
                TaskId = task.TaskId,
                ClientId = task.ClientId,
                Title = task.Title,
                Description = task.Description,
                Budget = task.Budget,
                Deadline = task.Deadline,
                Status = task.Status,
                CreatedAt = task.CreatedAt
            };
        }

        public async Task<IEnumerable<TaskDto>> GetTasksByClientIdAsync(int clientId)
        {
            return await _dbContext.Tasks
                .Where(t => t.ClientId == clientId)
                .AsNoTracking() // Good practice for read-only queries
                .Select(task => new TaskDto
                {
                    TaskId = task.TaskId,
                    ClientId = task.ClientId,
                    Title = task.Title,
                    Description = task.Description,
                    Budget = task.Budget,
                    Deadline = task.Deadline,
                    Status = task.Status,
                    CreatedAt = task.CreatedAt
                })
                .ToListAsync();
        }
    }
}
