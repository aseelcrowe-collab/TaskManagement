using System.ComponentModel.DataAnnotations;
using TaskManagement.Api.Models;

namespace TaskManagement.Api.DTOs;

/// <summary>Filtering, sorting and pagination options for GET /api/tasks.</summary>
public class TaskQueryParameters
{
    public TaskItemStatus? Status { get; set; }
    public TaskPriority? Priority { get; set; }

    /// <summary>Case-insensitive match against title and description.</summary>
    [MaxLength(200)]
    public string? Search { get; set; }

    /// <summary>Only tasks past their due date that are not Done.</summary>
    public bool? OverdueOnly { get; set; }

    public DateTime? DueBefore { get; set; }
    public DateTime? DueAfter { get; set; }

    /// <summary>createdat | duedate | priority | title | status (default: createdat).</summary>
    public string SortBy { get; set; } = "createdat";

    /// <summary>asc | desc (default: desc).</summary>
    public string SortOrder { get; set; } = "desc";

    [Range(1, int.MaxValue)]
    public int Page { get; set; } = 1;

    [Range(1, 100)]
    public int PageSize { get; set; } = 10;
}
