using System.ComponentModel.DataAnnotations;

namespace TaskManagement.Api.DTOs;

/// <summary>Payload for creating a user task.</summary>
public class CreateUserTaskDto
{
    [Required(ErrorMessage = "Title is required.")]
    [MinLength(1)]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(2000)]
    public string? Description { get; set; }

    [Required]
    public DateTime FromDate { get; set; }

    [Required]
    public DateTime ToDate { get; set; }

    [Required]
    public long StatusId { get; set; }
}
