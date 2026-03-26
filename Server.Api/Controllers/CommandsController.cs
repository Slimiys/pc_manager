using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Server.Api.Hubs;
using Server.Api.Models;
using Server.Application.Commands;
using Server.Domain.Commands;

namespace Server.Api.Controllers;

/// <summary>
/// Контроллер управления командами.
/// </summary>
[ApiController]
[Route("api/commands")]
[Authorize(Roles = "Operator,Admin")]
public sealed class CommandsController : ControllerBase
{
    private readonly ICommandService _commandService;
    private readonly IHubContext<CommandEventsHub> _hubContext;

    /// <summary>
    /// Создает контроллер команд.
    /// </summary>
    public CommandsController(ICommandService commandService, IHubContext<CommandEventsHub> hubContext)
    {
        _commandService = commandService;
        _hubContext = hubContext;
    }

    /// <summary>
    /// Выполняет команду и возвращает её состояние.
    /// </summary>
    /// <param name="request">Тело запроса.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    [HttpPost("execute")]
    public async Task<ActionResult<CommandExecutionResponseDto>> ExecuteAsync(
        [FromBody] ExecuteCommandRequestDto request,
        CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<CommandType>(request.Type, true, out var parsedType))
        {
            return ValidationProblem($"Unknown command type '{request.Type}'.");
        }

        var commandId = Guid.NewGuid();
        var requestedBy = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "anonymous";
        var execution = await _commandService.ExecuteAsync(
            new CommandRequest(commandId, parsedType, request.Payload, requestedBy),
            cancellationToken);

        await _hubContext.Clients.All.SendAsync("CommandStatusChanged", execution, cancellationToken);

        return Ok(ToDto(execution));
    }

    /// <summary>
    /// Возвращает статус выполнения команды.
    /// </summary>
    /// <param name="id">Идентификатор команды.</param>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<CommandExecutionResponseDto>> GetStatusAsync(Guid id)
    {
        var execution = await _commandService.GetByIdAsync(id);
        if (execution is null)
        {
            return NotFound();
        }

        return Ok(ToDto(execution));
    }

    private static CommandExecutionResponseDto ToDto(CommandExecution execution) =>
        new(execution.CommandId, execution.Type, execution.Status, execution.Result, execution.Error, execution.CreatedAt);
}
