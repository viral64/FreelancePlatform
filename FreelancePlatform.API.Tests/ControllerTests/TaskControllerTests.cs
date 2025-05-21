using FreelancePlatform.API.Controllers;
using FreelancePlatform.Application.Tasks;
using FreelancePlatform.Application.Tasks.Dtos;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Xunit;

namespace FreelancePlatform.API.Tests.ControllerTests
{
    public class TaskControllerTests
    {
        private readonly Mock<ITaskService> _mockTaskService;
        private readonly TaskController _controller;
        private const string TestUserId = "1"; // String because NameIdentifier claim is a string

        public TaskControllerTests()
        {
            _mockTaskService = new Mock<ITaskService>();
            _controller = new TaskController(_mockTaskService.Object);

            // Mock HttpContext and User for GetCurrentUserId()
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier, TestUserId)
            }, "mock"));

            _controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext() { User = user }
            };
        }

        [Fact]
        public async Task CreateTask_WithValidModel_ShouldReturnCreatedAtAction()
        {
            // Arrange
            var createTaskDto = new CreateTaskDto { Title = "New Task" };
            var createdTaskDto = new TaskDto { TaskId = 1, Title = "New Task", ClientId = int.Parse(TestUserId) };
            _mockTaskService.Setup(s => s.CreateTaskAsync(createTaskDto, int.Parse(TestUserId)))
                .ReturnsAsync(createdTaskDto);

            // Act
            var result = await _controller.CreateTask(createTaskDto);

            // Assert
            var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result);
            Assert.Equal(201, createdAtActionResult.StatusCode);
            Assert.Equal(nameof(_controller.GetTaskById), createdAtActionResult.ActionName);
            Assert.Equal(createdTaskDto.TaskId, createdAtActionResult.RouteValues["id"]);
            Assert.Equal(createdTaskDto, createdAtActionResult.Value);
        }

        [Fact]
        public async Task CreateTask_WithInvalidModel_ShouldReturnBadRequest()
        {
            // Arrange
            _controller.ModelState.AddModelError("Title", "Required");
            var createTaskDto = new CreateTaskDto { Description = "Missing title" }; // Model is invalid

            // Act
            var result = await _controller.CreateTask(createTaskDto);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.IsAssignableFrom<SerializableError>(badRequestResult.Value);
        }
        
        [Fact]
        public async Task CreateTask_WhenGetCurrentUserIdThrows_ShouldReturnUnauthorized()
        {
            // Arrange
            // Override the default valid user for this specific test
             var userWithoutNameId = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                // No NameIdentifier claim
            }, "mock"));
            _controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext() { User = userWithoutNameId }
            };

            var createTaskDto = new CreateTaskDto { Title = "New Task" };
            // No need to setup _mockTaskService as GetCurrentUserId will throw first

            // Act
            var result = await _controller.CreateTask(createTaskDto);

            // Assert
            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
            dynamic value = unauthorizedResult.Value;
            // Access the 'message' property from the anonymous object
            string message = value.GetType().GetProperty("message").GetValue(value, null);
            Assert.Equal("User ID not found in token or is invalid.", message);
        }


        [Fact]
        public async Task GetTaskById_WhenTaskExistsAndBelongsToUser_ShouldReturnOkResultWithTask()
        {
            // Arrange
            var taskId = 1;
            var taskDto = new TaskDto { TaskId = taskId, Title = "Test Task", ClientId = int.Parse(TestUserId) };
            _mockTaskService.Setup(s => s.GetTaskByIdAsync(taskId, int.Parse(TestUserId)))
                .ReturnsAsync(taskDto);

            // Act
            var result = await _controller.GetTaskById(taskId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(taskDto, okResult.Value);
        }

        [Fact]
        public async Task GetTaskById_WhenTaskNotFoundOrNotAuthorized_ShouldReturnNotFoundResult()
        {
            // Arrange
            var taskId = 1;
            _mockTaskService.Setup(s => s.GetTaskByIdAsync(taskId, int.Parse(TestUserId)))
                .ReturnsAsync((TaskDto)null); // Task service returns null

            // Act
            var result = await _controller.GetTaskById(taskId);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            dynamic value = notFoundResult.Value;
            string message = value.GetType().GetProperty("message").GetValue(value, null);
            Assert.Equal($"Task with ID {taskId} not found or access denied.", message);
        }
        
        [Fact]
        public async Task GetTaskById_WhenGetCurrentUserIdThrows_ShouldReturnUnauthorized()
        {
            // Arrange
            var taskId = 1;
            var userWithoutNameId = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]{}, "mock"));
            _controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext() { User = userWithoutNameId }
            };
            // No need to setup _mockTaskService as GetCurrentUserId will throw first

            // Act
            var result = await _controller.GetTaskById(taskId);

            // Assert
            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
            dynamic value = unauthorizedResult.Value;
            string message = value.GetType().GetProperty("message").GetValue(value, null);
            Assert.Equal("User ID not found in token or is invalid.", message);
        }

        [Fact]
        public async Task GetClientTasks_ShouldReturnOkResultWithListOfTasks()
        {
            // Arrange
            var tasks = new List<TaskDto>
            {
                new TaskDto { TaskId = 1, Title = "Task 1", ClientId = int.Parse(TestUserId) },
                new TaskDto { TaskId = 2, Title = "Task 2", ClientId = int.Parse(TestUserId) }
            };
            _mockTaskService.Setup(s => s.GetTasksByClientIdAsync(int.Parse(TestUserId)))
                .ReturnsAsync(tasks);

            // Act
            var result = await _controller.GetClientTasks();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(tasks, okResult.Value);
        }

        [Fact]
        public async Task GetClientTasks_WhenGetCurrentUserIdThrows_ShouldReturnUnauthorized()
        {
            // Arrange
            var userWithoutNameId = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]{}, "mock"));
             _controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext() { User = userWithoutNameId }
            };
            // No need to setup _mockTaskService as GetCurrentUserId will throw first

            // Act
            var result = await _controller.GetClientTasks();

            // Assert
            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
            dynamic value = unauthorizedResult.Value;
            string message = value.GetType().GetProperty("message").GetValue(value, null);
            Assert.Equal("User ID not found in token or is invalid.", message);
        }
    }
}
