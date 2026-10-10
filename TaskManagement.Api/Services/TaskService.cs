using Microsoft.EntityFrameworkCore;
using TaskManagement.Api.Data;
using TaskManagement.Api.DTOs;
using TaskManagement.Api.Models;
using Task = System.Threading.Tasks.Task;

namespace TaskManagement.Api.Services;

public class TaskService(AppDbContext db) : ITaskService
{
    public async Task<PagedResult<TaskResponseDto>> GetPagedAsync(
        TaskQueryParameters query, CancellationToken cancellationToken = default)
    {
        IQueryable<TaskItem> tasks = db.Tasks.AsNoTracking();

        if (query.Status.HasValue)
            tasks = tasks.Where(t => t.Status == query.Status.Value);

        if (query.Priority.HasValue)
            tasks = tasks.Where(t => t.Priority == query.Priority.Value);

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var term = query.Search.Trim().ToLower();
            tasks = tasks.Where(t => t.Title.ToLower().Contains(term) ||
                                     (t.Description != null && t.Description.ToLower().Contains(term)));
        }

        var now = DateTime.UtcNow;
        if (query.OverdueOnly == true)
            tasks = tasks.Where(t => t.DueDate.HasValue && t.DueDate.Value < now && t.Status != TaskItemStatus.Done);

        if (query.DueBefore.HasValue)
            tasks = tasks.Where(t => t.DueDate.HasValue && t.DueDate.Value < query.DueBefore.Value);

        if (query.DueAfter.HasValue)
            tasks = tasks.Where(t => t.DueDate.HasValue && t.DueDate.Value > query.DueAfter.Value);

        var descending = !string.Equals(query.SortOrder, "asc", StringComparison.OrdinalIgnoreCase);
        tasks = (query.SortBy?.ToLowerInvariant()) switch
        {
            "duedate" => descending
                ? tasks.OrderByDescending(t => t.DueDate).ThenByDescending(t => t.Id)
                : tasks.OrderBy(t => t.DueDate).ThenBy(t => t.Id),
            "priority" => descending
                ? tasks.OrderByDescending(t => t.Priority).ThenByDescending(t => t.Id)
                : tasks.OrderBy(t => t.Priority).ThenBy(t => t.Id),
            "title" => descending
                ? tasks.OrderByDescending(t => t.Title).ThenByDescending(t => t.Id)
                : tasks.OrderBy(t => t.Title).ThenBy(t => t.Id),
            "status" => descending
                ? tasks.OrderByDescending(t => t.Status).ThenByDescending(t => t.Id)
                : tasks.OrderBy(t => t.Status).ThenBy(t => t.Id),
            _ => descending
                ? tasks.OrderByDescending(t => t.CreatedAt).ThenByDescending(t => t.Id)
                : tasks.OrderBy(t => t.CreatedAt).ThenBy(t => t.Id),
        };

        var totalCount = await tasks.CountAsync(cancellationToken);
        var items = await tasks
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<TaskResponseDto>
        {
            Items = items.Select(TaskResponseDto.FromEntity).ToList(),
            Page = query.Page,
            PageSize = query.PageSize,
            TotalCount = totalCount
        };
    }

    public async Task<TaskResponseDto> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var task = await FindOrThrowAsync(id, cancellationToken);
        return TaskResponseDto.FromEntity(task);
    }

    public async Task<TaskResponseDto> CreateAsync(CreateTaskDto dto, CancellationToken cancellationToken = default)
    {
        var task = new TaskItem
        {
            Title = dto.Title.Trim(),
            Description = string.IsNullOrWhiteSpace(dto.Description) ? null : dto.Description.Trim(),
            Status = dto.Status,
            Priority = dto.Priority,
            DueDate = dto.DueDate,
            CreatedAt = DateTime.UtcNow
        };

        db.Tasks.Add(task);
        await db.SaveChangesAsync(cancellationToken);
        return TaskResponseDto.FromEntity(task);
    }

    public async Task<TaskResponseDto> UpdateAsync(int id, UpdateTaskDto dto, CancellationToken cancellationToken = default)
    {
        var task = await FindOrThrowAsync(id, cancellationToken);

        task.Title = dto.Title.Trim();
        task.Description = string.IsNullOrWhiteSpace(dto.Description) ? null : dto.Description.Trim();
        task.Status = dto.Status;
        task.Priority = dto.Priority;
        task.DueDate = dto.DueDate;
        task.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync(cancellationToken);
        return TaskResponseDto.FromEntity(task);
    }

    public async Task<TaskResponseDto> UpdateStatusAsync(int id, TaskItemStatus status, CancellationToken cancellationToken = default)
    {
        var task = await FindOrThrowAsync(id, cancellationToken);

        task.Status = status;
        task.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync(cancellationToken);
        return TaskResponseDto.FromEntity(task);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var task = await FindOrThrowAsync(id, cancellationToken);
        db.Tasks.Remove(task);
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task<TaskStatsDto> GetStatsAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var groups = await db.Tasks
            .AsNoTracking()
            .GroupBy(t => t.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToListAsync(cancellationToken);

        var overdue = await db.Tasks
            .AsNoTracking()
            .CountAsync(t => t.DueDate.HasValue && t.DueDate.Value < now && t.Status != TaskItemStatus.Done,
                cancellationToken);

        int CountOf(TaskItemStatus status) => groups.FirstOrDefault(g => g.Status == status)?.Count ?? 0;

        return new TaskStatsDto
        {
            Todo = CountOf(TaskItemStatus.Todo),
            InProgress = CountOf(TaskItemStatus.InProgress),
            Done = CountOf(TaskItemStatus.Done),
            Total = groups.Sum(g => g.Count),
            Overdue = overdue
        };
    }

    private async Task<TaskItem> FindOrThrowAsync(int id, CancellationToken cancellationToken)
    {
        var task = await db.Tasks.FindAsync([id], cancellationToken);
        if (task is null)
            throw new KeyNotFoundException($"Task with id {id} was not found.");
        return task;
    }
}
