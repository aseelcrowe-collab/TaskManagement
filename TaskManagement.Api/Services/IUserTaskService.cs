using TaskManagement.Api.DTOs;
using Task = System.Threading.Tasks.Task;

namespace TaskManagement.Api.Services;

/// <summary>Task management scoped to the logged-in user.</summary>
public interface IUserTaskService
{
    Task<List<TaskDetailDto>> GetByCriteriaAsync(long userId, string? title, long? statusId, CancellationToken cancellationToken = default);
    Task<TaskDetailDto> GetByIdAsync(long id, long userId, CancellationToken cancellationToken = default);
    Task<TaskDetailDto> AddAsync(long userId, CreateUserTaskDto dto, CancellationToken cancellationToken = default);
    Task<TaskDetailDto> UpdateAsync(long userId, UpdateUserTaskDto dto, CancellationToken cancellationToken = default);
    Task DeleteAsync(long id, long userId, CancellationToken cancellationToken = default);
}
