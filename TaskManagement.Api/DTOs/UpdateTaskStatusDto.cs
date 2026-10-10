using System.ComponentModel.DataAnnotations;
using TaskManagement.Api.Models;

namespace TaskManagement.Api.DTOs;

/// <summary>Payload for changing only a task's status.</summary>
public class UpdateTaskItemStatusDto
{
    [Required]
    public TaskItemStatus Status { get; set; }
}
