using FreelancePlatform.Application.Tasks;
using FreelancePlatform.Application.Tasks.Dtos;
using FreelancePlatform.Domain.Entities;
using FreelancePlatform.Persistence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace FreelancePlatform.Application.Tests.ServiceTests
{
    public class TaskServiceTests
    {
        private DbContextOptions<ViralDbContext> _dbContextOptions;

        public TaskServiceTests()
        {
            // Use a new in-memory database for each test
            _dbContextOptions = new DbContextOptionsBuilder<ViralDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
        }

        private ViralDbContext CreateContext() => new ViralDbContext(_dbContextOptions);

        [Fact]
        public async Task CreateTaskAsync_ShouldAddTaskToDatabaseAndReturnTaskDto()
        {
            // Arrange
            await using var context = CreateContext();
            var taskService = new TaskService(context);
            var createTaskDto = new CreateTaskDto { Title = "Test Task", Description = "Test Description", Budget = 100m, Deadline = DateTime.UtcNow.AddDays(7) };
            var clientId = 1;

            // Act
            var result = await taskService.CreateTaskAsync(createTaskDto, clientId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(createTaskDto.Title, result.Title);
            Assert.Equal(createTaskDto.Description, result.Description);
            Assert.Equal(createTaskDto.Budget, result.Budget);
            Assert.Equal(createTaskDto.Deadline, result.Deadline);
            Assert.Equal(clientId, result.ClientId);
            Assert.Equal("Open", result.Status);
            Assert.True(result.TaskId > 0);
            Assert.True(result.CreatedAt > DateTime.MinValue);

            var taskInDb = await context.Tasks.FindAsync(result.TaskId);
            Assert.NotNull(taskInDb);
            Assert.Equal(createTaskDto.Title, taskInDb.Title);
        }

        [Fact]
        public async Task GetTaskByIdAsync_ShouldReturnTaskDto_WhenTaskExistsAndBelongsToClient()
        {
            // Arrange
            await using var context = CreateContext();
            var taskService = new TaskService(context);
            var clientId = 1;
            var task = new Domain.Entities.Task { Title = "Existing Task", ClientId = clientId, CreatedAt = DateTime.UtcNow, Status = "Open" };
            context.Tasks.Add(task);
            await context.SaveChangesAsync();

            // Act
            var result = await taskService.GetTaskByIdAsync(task.TaskId, clientId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(task.TaskId, result.TaskId);
            Assert.Equal(task.Title, result.Title);
            Assert.Equal(clientId, result.ClientId);
        }

        [Fact]
        public async Task GetTaskByIdAsync_ShouldReturnNull_WhenTaskDoesNotExist()
        {
            // Arrange
            await using var context = CreateContext();
            var taskService = new TaskService(context);
            var clientId = 1;

            // Act
            var result = await taskService.GetTaskByIdAsync(999, clientId); // Non-existent TaskId

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetTaskByIdAsync_ShouldReturnNull_WhenTaskExistsButBelongsToDifferentClient()
        {
            // Arrange
            await using var context = CreateContext();
            var taskService = new TaskService(context);
            var clientAId = 1;
            var clientBId = 2;
            var taskForClientA = new Domain.Entities.Task { Title = "Client A Task", ClientId = clientAId, CreatedAt = DateTime.UtcNow, Status = "Open" };
            context.Tasks.Add(taskForClientA);
            await context.SaveChangesAsync();

            // Act
            var result = await taskService.GetTaskByIdAsync(taskForClientA.TaskId, clientBId);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetTasksByClientIdAsync_ShouldReturnAllTasksForClient_AndNotForOthers()
        {
            // Arrange
            await using var context = CreateContext();
            var taskService = new TaskService(context);
            var clientAId = 1;
            var clientBId = 2;

            context.Tasks.AddRange(
                new Domain.Entities.Task { Title = "Client A Task 1", ClientId = clientAId, CreatedAt = DateTime.UtcNow, Status = "Open" },
                new Domain.Entities.Task { Title = "Client B Task 1", ClientId = clientBId, CreatedAt = DateTime.UtcNow, Status = "Open" },
                new Domain.Entities.Task { Title = "Client A Task 2", ClientId = clientAId, CreatedAt = DateTime.UtcNow, Status = "Open" }
            );
            await context.SaveChangesAsync();

            // Act
            var result = await taskService.GetTasksByClientIdAsync(clientAId);

            // Assert
            Assert.NotNull(result);
            var taskDtos = result.ToList();
            Assert.Equal(2, taskDtos.Count);
            Assert.All(taskDtos, dto => Assert.Equal(clientAId, dto.ClientId));
            Assert.Contains(taskDtos, dto => dto.Title == "Client A Task 1");
            Assert.Contains(taskDtos, dto => dto.Title == "Client A Task 2");
        }

        [Fact]
        public async Task GetTasksByClientIdAsync_ShouldReturnEmptyList_WhenClientHasNoTasks()
        {
            // Arrange
            await using var context = CreateContext();
            var taskService = new TaskService(context);
            var clientIdWithNoTasks = 3;
            var clientWithTasks = 4; // Ensure DB is not empty
             context.Tasks.Add(new Domain.Entities.Task { Title = "Some other task", ClientId = clientWithTasks, CreatedAt = DateTime.UtcNow, Status = "Open" });
            await context.SaveChangesAsync();


            // Act
            var result = await taskService.GetTasksByClientIdAsync(clientIdWithNoTasks);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }
    }
}
