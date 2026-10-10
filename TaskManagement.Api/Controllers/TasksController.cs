using Microsoft.AspNetCore.Mvc;
using TaskManagement.Api.DTOs;
using TaskManagement.Api.Services;

namespace TaskManagement.Api.Controllers;

/// <summary>CRUD endpoints for tasks.</summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class TasksController(ITaskService taskService) : ControllerBase
{
    /// <summary>Get a filtered, sorted, paginated list of tasks.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<TaskResponseDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<TaskResponseDto>>> GetTasks(
        [FromQuery] TaskQueryParameters query, CancellationToken cancellationToken)
    {
        var result = await taskService.GetPagedAsync(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>Get aggregate task counts.</summary>
    [HttpGet("stats")]
    [ProducesResponseType(typeof(TaskStatsDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<TaskStatsDto>> GetStats(CancellationToken cancellationToken)
    {
        var stats = await taskService.GetStatsAsync(cancellationToken);
        return Ok(stats);
    }

    /// <summary>Get a single task by id.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(TaskResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TaskResponseDto>> GetTask(int id, CancellationToken cancellationToken)
    {
        var task = await taskService.GetByIdAsync(id, cancellationToken);
        return Ok(task);
    }

    /// <summary>Create a new task.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(TaskResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<TaskResponseDto>> CreateTask(
        [FromBody] CreateTaskDto dto, CancellationToken cancellationToken)
    {
        var created = await taskService.CreateAsync(dto, cancellationToken);
        return CreatedAtAction(nameof(GetTask), new { id = created.Id }, created);
    }

    /// <summary>Fully replace a task.</summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(TaskResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TaskResponseDto>> UpdateTask(
        int id, [FromBody] UpdateTaskDto dto, CancellationToken cancellationToken)
    {
        var updated = await taskService.UpdateAsync(id, dto, cancellationToken);
        return Ok(updated);
    }

    /// <summary>Change only a task's status.</summary>
    [HttpPatch("{id:int}/status")]
    [ProducesResponseType(typeof(TaskResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TaskResponseDto>> UpdateTaskItemStatus(
        int id, [FromBody] UpdateTaskItemStatusDto dto, CancellationToken cancellationToken)
    {
        var updated = await taskService.UpdateStatusAsync(id, dto.Status, cancellationToken);
        return Ok(updated);
    }

    /// <summary>Delete a task.</summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteTask(int id, CancellationToken cancellationToken)
    {
        await taskService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}
