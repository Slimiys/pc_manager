using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Server.Api.Hubs;

/// <summary>
/// Hub для realtime-событий по выполнению команд и оповещениям.
/// </summary>
[Authorize(Roles = "Operator,Admin")]
public sealed class CommandEventsHub : Hub
{
}
