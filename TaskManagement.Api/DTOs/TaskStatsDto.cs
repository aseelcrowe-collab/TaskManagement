namespace TaskManagement.Api.DTOs;

/// <summary>Aggregate counts returned by GET /api/tasks/stats.</summary>
public class TaskStatsDto
{
    public int Total { get; set; }
    public int Todo { get; set; }
    public int InProgress { get; set; }
    public int Done { get; set; }
    public int Overdue { get; set; }
}
