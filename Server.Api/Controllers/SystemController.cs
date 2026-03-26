using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Server.Application.Contracts;

namespace Server.Api.Controllers;

/// <summary>
/// Контроллер системной информации.
/// </summary>
[ApiController]
[Route("api/system")]
[Authorize(Roles = "Operator,Admin")]
public sealed class SystemController : ControllerBase
{
    private readonly ISystemInfoProvider _systemInfoProvider;

    /// <summary>
    /// Создает контроллер системной информации.
    /// </summary>
    public SystemController(ISystemInfoProvider systemInfoProvider)
    {
        _systemInfoProvider = systemInfoProvider;
    }

    /// <summary>
    /// Возвращает текущую информацию о системе.
    /// </summary>
    [HttpGet("info")]
    public async Task<IActionResult> GetInfoAsync()
    {
        var data = await _systemInfoProvider.GetAsync();
        return Ok(data);
    }
}
