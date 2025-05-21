namespace FreelancePlatform.Application.Tasks.Dtos;

public class CreateTaskDto
{
    public string Title { get; set; }
    public string? Description { get; set; }
    public decimal? Budget { get; set; }
    public DateTime? Deadline { get; set; }
    // ClientId will be set from the authenticated user context in the service/controller
}
