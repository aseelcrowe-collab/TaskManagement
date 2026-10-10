using TaskManagement.Api.DTOs;
using Task = System.Threading.Tasks.Task;

namespace TaskManagement.Api.Services;

/// <summary>User registration and login.</summary>
public interface IAuthService
{
    Task<AuthResponseDto> SignupAsync(SignupDto dto, CancellationToken cancellationToken = default);
    Task<AuthResponseDto> LoginAsync(LoginDto dto, CancellationToken cancellationToken = default);
}
