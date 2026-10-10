namespace TaskManagement.Api.DTOs;

/// <summary>Task details returned by the user task endpoints.</summary>
public class TaskDetailDto
{
    public long Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public DateTime FromDate { get; set; }

    public DateTime ToDate { get; set; }

    public long StatusId { get; set; }

    public string StatusName { get; set; } = string.Empty;

    public long UserId { get; set; }
}
