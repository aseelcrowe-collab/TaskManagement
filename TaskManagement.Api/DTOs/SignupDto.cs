using System.ComponentModel.DataAnnotations;

namespace TaskManagement.Api.DTOs;

/// <summary>Payload for registering a new user.</summary>
public class SignupDto
{
    [Required(ErrorMessage = "Username is required.")]
    [MinLength(3)]
    [MaxLength(100)]
    public string Username { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Email is not valid.")]
    [MaxLength(256)]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password is required.")]
    [MinLength(6, ErrorMessage = "Password must be at least 6 characters.")]
    [MaxLength(100)]
    public string Password { get; set; } = string.Empty;
}
