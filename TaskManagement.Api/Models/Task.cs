using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TaskManagement.Api.Models;

/// <summary>Task entity persisted in SQL Server.</summary>
public class Task
{
    public long Id { get; set; }

    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(2000)]
    public string? Description { get; set; }

    public DateTime FromDate { get; set; }

    public DateTime ToDate { get; set; }

    public long StatusId { get; set; }

    [ForeignKey(nameof(StatusId))]
    public Lookup Status { get; set; } = null!;

    public long UserId { get; set; }

    [ForeignKey(nameof(UserId))]
    public User User { get; set; } = null!;
}
