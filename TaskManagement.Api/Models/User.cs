using System.ComponentModel.DataAnnotations;

namespace TaskManagement.Api.Models;

/// <summary>User entity persisted in SQL Server.</summary>
public class User
{
    public long Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Username { get; set; } = string.Empty;

    [Required]
    [MaxLength(256)]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MaxLength(512)]
    public string HashedPassword { get; set; } = string.Empty;
}
