namespace FreelancePlatform.Application.Tasks.Dtos;

public class TaskDto
{
    public int TaskId { get; set; }
    public int ClientId { get; set; }
    public string Title { get; set; }
    public string? Description { get; set; }
    public decimal? Budget { get; set; }
    public DateTime? Deadline { get; set; }
    public string? Status { get; set; }
    public DateTime CreatedAt { get; set; }
}
