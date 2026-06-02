using AIScaling.Infrastructure.Metrics;
using AIScaling.Shared.DTOs;
using AIScaling.Shared.Responses;
using Microsoft.AspNetCore.Mvc;
using UserService.Application.Interfaces;

namespace UserService.Api.Controllers;

/// <summary>User CRUD API endpoints.</summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public sealed class UsersController : ControllerBase
{
    private readonly IUserAppService _userService;
    private readonly RuntimeMetricsCollector _metrics;

    public UsersController(IUserAppService userService, RuntimeMetricsCollector metrics)
    {
        _userService = userService;
        _metrics = metrics;
    }

    /// <summary>Get all users.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<UserDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<UserDto>>>> GetAll(CancellationToken cancellationToken)
    {
        RecordMetrics();
        var users = await _userService.GetAllAsync(cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<UserDto>>.Ok(users));
    }

    /// <summary>Get user by id.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<UserDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<UserDto>>> GetById(int id, CancellationToken cancellationToken)
    {
        RecordMetrics();
        var user = await _userService.GetByIdAsync(id, cancellationToken);
        if (user is null)
            return NotFound(ApiResponse<UserDto>.Fail($"User {id} not found."));
        return Ok(ApiResponse<UserDto>.Ok(user));
    }

    /// <summary>Create a new user.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<UserDto>), StatusCodes.Status201Created)]
    public async Task<ActionResult<ApiResponse<UserDto>>> Create(
        [FromBody] CreateUserDto dto,
        CancellationToken cancellationToken)
    {
        RecordMetrics();
        var user = await _userService.CreateAsync(dto, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = user.Id }, ApiResponse<UserDto>.Ok(user));
    }

    /// <summary>Update an existing user.</summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<UserDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<UserDto>>> Update(
        int id,
        [FromBody] UpdateUserDto dto,
        CancellationToken cancellationToken)
    {
        RecordMetrics();
        var user = await _userService.UpdateAsync(id, dto, cancellationToken);
        if (user is null)
            return NotFound(ApiResponse<UserDto>.Fail($"User {id} not found."));
        return Ok(ApiResponse<UserDto>.Ok(user));
    }

    /// <summary>Delete a user.</summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        RecordMetrics();
        var deleted = await _userService.DeleteAsync(id, cancellationToken);
        if (!deleted) return NotFound(ApiResponse<object>.Fail($"User {id} not found."));
        return NoContent();
    }

    private void RecordMetrics() => _metrics.RecordRequest(0);
}
