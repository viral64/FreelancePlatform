using FreelancePlatform.Application.Tasks.Dtos;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FreelancePlatform.Application.Tasks
{
    public interface ITaskService
    {
        Task<TaskDto> CreateTaskAsync(CreateTaskDto taskDto, int clientId);
        Task<TaskDto?> GetTaskByIdAsync(int taskId, int clientId); // Return TaskDto? to indicate not found
        Task<IEnumerable<TaskDto>> GetTasksByClientIdAsync(int clientId);
    }
}
