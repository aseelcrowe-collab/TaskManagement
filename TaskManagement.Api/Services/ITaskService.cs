using TaskManagement.Api.DTOs;
using TaskManagement.Api.Models;
using Task = System.Threading.Tasks.Task;

namespace TaskManagement.Api.Services;

/// <summary>Business logic for managing tasks.</summary>
public interface ITaskService
{
    Task<PagedResult<TaskResponseDto>> GetPagedAsync(TaskQueryParameters query, CancellationToken cancellationToken = default);
    Task<TaskResponseDto> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<TaskResponseDto> CreateAsync(CreateTaskDto dto, CancellationToken cancellationToken = default);
    Task<TaskResponseDto> UpdateAsync(int id, UpdateTaskDto dto, CancellationToken cancellationToken = default);
    Task<TaskResponseDto> UpdateStatusAsync(int id, TaskItemStatus status, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
    Task<TaskStatsDto> GetStatsAsync(CancellationToken cancellationToken = default);
}
