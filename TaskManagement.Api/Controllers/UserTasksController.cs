using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskManagement.Api.DTOs;
using TaskManagement.Api.Services;

namespace TaskManagement.Api.Controllers;

/// <summary>User task endpoints (scoped to the logged-in user).</summary>
/// <remarks>Shares the "api/tasks" prefix with <see cref="TasksController"/>; action routes don't overlap.</remarks>
[ApiController]
[Route("api/tasks")]
[Authorize]
[Produces("application/json")]
public class UserTasksController(IUserTaskService taskService) : ControllerBase
{
    /// <summary>Get the logged-in user's tasks, optionally filtered by title and/or status.</summary>
    [HttpGet("GetByCriteria")]
    [ProducesResponseType(typeof(List<TaskDetailDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<List<TaskDetailDto>>> GetByCriteria(
        [FromQuery] string? title, [FromQuery] long? statusId, CancellationToken cancellationToken)
    {
        if (!TryGetUserId(out var userId))
            return Unauthorized();
        return Ok(await taskService.GetByCriteriaAsync(userId, title, statusId, cancellationToken));
    }

    /// <summary>Get a single task by id (only if owned by the logged-in user).</summary>
    [HttpGet("GetById/{id:long}")]
    [ProducesResponseType(typeof(TaskDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TaskDetailDto>> GetById(long id, CancellationToken cancellationToken)
    {
        if (!TryGetUserId(out var userId))
            return Unauthorized();
        try
        {
            return Ok(await taskService.GetByIdAsync(id, userId, cancellationToken));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(BadRequestProblem(ex.Message));
        }
    }

    /// <summary>Create a new task for the logged-in user.</summary>
    [HttpPost("Add")]
    [ProducesResponseType(typeof(TaskDetailDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<TaskDetailDto>> Add(
        [FromBody] CreateUserTaskDto dto, CancellationToken cancellationToken)
    {
        if (!TryGetUserId(out var userId))
            return Unauthorized();
        try
        {
            var created = await taskService.AddAsync(userId, dto, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(BadRequestProblem(ex.Message));
        }
    }

    /// <summary>Update an existing task (only if owned by the logged-in user).</summary>
    [HttpPut("Update")]
    [ProducesResponseType(typeof(TaskDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TaskDetailDto>> Update(
        [FromBody] UpdateUserTaskDto dto, CancellationToken cancellationToken)
    {
        if (!TryGetUserId(out var userId))
            return Unauthorized();
        try
        {
            return Ok(await taskService.UpdateAsync(userId, dto, cancellationToken));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(BadRequestProblem(ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(BadRequestProblem(ex.Message));
        }
    }

    /// <summary>Delete a task (only if owned by the logged-in user).</summary>
    [HttpDelete("Delete/{id:long}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(long id, CancellationToken cancellationToken)
    {
        if (!TryGetUserId(out var userId))
            return Unauthorized();
        try
        {
            await taskService.DeleteAsync(id, userId, cancellationToken);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(BadRequestProblem(ex.Message));
        }
    }

    private bool TryGetUserId(out long userId)
    {
        userId = 0;
        return long.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out userId);
    }

    private ProblemDetails BadRequestProblem(string detail) => new()
    {
        Status = StatusCodes.Status400BadRequest,
        Title = "Request failed.",
        Detail = detail,
        Instance = HttpContext.Request.Path
    };
}
