using Microsoft.EntityFrameworkCore;
using TaskManagement.Api.Data;
using TaskManagement.Api.DTOs;
using Task = System.Threading.Tasks.Task;
using TaskEntity = TaskManagement.Api.Models.Task;

namespace TaskManagement.Api.Services;

public class UserTaskService(AppDbContext db) : IUserTaskService
{
    public async Task<List<TaskDetailDto>> GetByCriteriaAsync(
        long userId, string? title, long? statusId, CancellationToken cancellationToken = default)
    {
        var query = db.UserTasks.AsNoTracking().Where(t => t.UserId == userId);

        if (!string.IsNullOrWhiteSpace(title))
        {
            var term = title.Trim();
            query = query.Where(t => t.Title.Contains(term));
        }

        if (statusId.HasValue)
            query = query.Where(t => t.StatusId == statusId.Value);

        return await query
            .OrderBy(t => t.FromDate)
            .ThenBy(t => t.Id)
            .Select(t => new TaskDetailDto
            {
                Id = t.Id,
                Title = t.Title,
                Description = t.Description,
                FromDate = t.FromDate,
                ToDate = t.ToDate,
                StatusId = t.StatusId,
                StatusName = t.Status.Name,
                UserId = t.UserId
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<TaskDetailDto> GetByIdAsync(long id, long userId, CancellationToken cancellationToken = default)
    {
        var task = await FindOrThrowAsync(id, cancellationToken);
        EnsureOwnedBy(task, userId);
        return await ToDetailAsync(task.Id, cancellationToken);
    }

    public async Task<TaskDetailDto> AddAsync(long userId, CreateUserTaskDto dto, CancellationToken cancellationToken = default)
    {
        ValidateDates(dto.FromDate, dto.ToDate);
        await EnsureStatusExistsAsync(dto.StatusId, cancellationToken);

        var task = new TaskEntity
        {
            Title = dto.Title.Trim(),
            Description = string.IsNullOrWhiteSpace(dto.Description) ? null : dto.Description.Trim(),
            FromDate = dto.FromDate,
            ToDate = dto.ToDate,
            StatusId = dto.StatusId,
            UserId = userId
        };

        db.UserTasks.Add(task);
        await db.SaveChangesAsync(cancellationToken);
        return await ToDetailAsync(task.Id, cancellationToken);
    }

    public async Task<TaskDetailDto> UpdateAsync(long userId, UpdateUserTaskDto dto, CancellationToken cancellationToken = default)
    {
        var task = await FindOrThrowAsync(dto.Id, cancellationToken);
        EnsureOwnedBy(task, userId);

        ValidateDates(dto.FromDate, dto.ToDate);
        await EnsureStatusExistsAsync(dto.StatusId, cancellationToken);

        task.Title = dto.Title.Trim();
        task.Description = string.IsNullOrWhiteSpace(dto.Description) ? null : dto.Description.Trim();
        task.FromDate = dto.FromDate;
        task.ToDate = dto.ToDate;
        task.StatusId = dto.StatusId;

        await db.SaveChangesAsync(cancellationToken);
        return await ToDetailAsync(task.Id, cancellationToken);
    }

    public async Task DeleteAsync(long id, long userId, CancellationToken cancellationToken = default)
    {
        var task = await FindOrThrowAsync(id, cancellationToken);
        EnsureOwnedBy(task, userId);

        db.UserTasks.Remove(task);
        await db.SaveChangesAsync(cancellationToken);
    }

    private async Task<TaskEntity> FindOrThrowAsync(long id, CancellationToken cancellationToken)
    {
        var task = await db.UserTasks.FindAsync([id], cancellationToken);
        if (task is null)
            throw new KeyNotFoundException($"Task with id {id} was not found.");
        return task;
    }

    private static void EnsureOwnedBy(TaskEntity task, long userId)
    {
        if (task.UserId != userId)
            throw new InvalidOperationException("Task is assigned to another user.");
    }

    private static void ValidateDates(DateTime fromDate, DateTime toDate)
    {
        if (fromDate == default || toDate == default)
            throw new ArgumentException("FromDate and ToDate are required.");
        if (fromDate > toDate)
            throw new ArgumentException("FromDate must be less than or equal to ToDate.");
    }

    private async Task EnsureStatusExistsAsync(long statusId, CancellationToken cancellationToken)
    {
        if (!await db.Lookups.AnyAsync(l => l.Id == statusId, cancellationToken))
            throw new ArgumentException($"Status with id {statusId} does not exist.");
    }

    private async Task<TaskDetailDto> ToDetailAsync(long id, CancellationToken cancellationToken)
    {
        return await db.UserTasks
            .AsNoTracking()
            .Where(t => t.Id == id)
            .Select(t => new TaskDetailDto
            {
                Id = t.Id,
                Title = t.Title,
                Description = t.Description,
                FromDate = t.FromDate,
                ToDate = t.ToDate,
                StatusId = t.StatusId,
                StatusName = t.Status.Name,
                UserId = t.UserId
            })
            .SingleAsync(cancellationToken);
    }
}
