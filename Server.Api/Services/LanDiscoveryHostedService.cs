using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Hosting;

namespace Server.Api.Services;

/// <summary>
/// Фоновый UDP-discovery сервис для поиска API в локальной сети.
/// </summary>
public sealed class LanDiscoveryHostedService : BackgroundService
{
    private const int DefaultPort = 37020;
    private const int MaxDatagramSize = 4096;
    private const int ClockSkewSeconds = 30;
    private const string PlaceholderKeyPrefix = "CHANGE_ME";

    private readonly IConfiguration _configuration;
    private readonly ILogger<LanDiscoveryHostedService> _logger;

    /// <summary>
    /// Создаёт сервис LAN-discovery.
    /// </summary>
    /// <param name="configuration">Конфигурация приложения.</param>
    /// <param name="logger">Логгер.</param>
    public LanDiscoveryHostedService(
        IConfiguration configuration,
        ILogger<LanDiscoveryHostedService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var enabled = _configuration.GetValue("Discovery:Enabled", true);
        if (!enabled)
        {
            _logger.LogInformation("LAN discovery отключён (Discovery:Enabled=false).");
            return;
        }

        var sharedKey = _configuration["Discovery:SharedKey"];
        if (string.IsNullOrWhiteSpace(sharedKey))
        {
            _logger.LogWarning("LAN discovery отключён: не задан Discovery:SharedKey.");
            return;
        }
        if (sharedKey.StartsWith(PlaceholderKeyPrefix, StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogWarning("LAN discovery отключён: используйте непустой уникальный Discovery:SharedKey.");
            return;
        }

        var pairId = _configuration["Discovery:PairId"] ?? "default";
        var listenPort = _configuration.GetValue("Discovery:UdpPort", DefaultPort);
        var apiPort = _configuration.GetValue("Discovery:ApiPort", _configuration.GetValue("Server:Port", 8080));
        var agentName = _configuration["Discovery:AgentName"] ?? Environment.MachineName;

        using var udpClient = new UdpClient(new IPEndPoint(IPAddress.Any, listenPort))
        {
            EnableBroadcast = true
        };

        _logger.LogInformation(
            "LAN discovery слушает UDP {Port}; pairId={PairId}; agentName={AgentName}.",
            listenPort,
            pairId,
            agentName);

        while (!stoppingToken.IsCancellationRequested)
        {
            UdpReceiveResult received;
            try
            {
                received = await udpClient.ReceiveAsync(stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "Ошибка UDP-приёма discovery.");
                continue;
            }

            if (received.Buffer.Length == 0 || received.Buffer.Length > MaxDatagramSize)
            {
                continue;
            }

            DiscoveryRequestDto? request;
            try
            {
                request = JsonSerializer.Deserialize<DiscoveryRequestDto>(received.Buffer);
            }
            catch
            {
                continue;
            }

            if (request is null || !string.Equals(request.PairId, pairId, StringComparison.Ordinal))
            {
                continue;
            }

            if (!IsTimestampValid(request.TimestampUnix))
            {
                continue;
            }

            var expectedProof = ComputeHmac(
                sharedKey,
                $"{request.Version}|{request.PairId}|{request.Nonce}|{request.TimestampUnix}");
            if (!FixedTimeEquals(expectedProof, request.Proof))
            {
                continue;
            }

            if (!TryResolveServerAddressForClient(received.RemoteEndPoint, out var host))
            {
                _logger.LogDebug("Не удалось определить IPv4-адрес сервера для клиента {Client}.", received.RemoteEndPoint);
                continue;
            }
            var apiUrl = $"http://{host}:{apiPort}";
            var ts = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            var responseProof = ComputeHmac(
                sharedKey,
                $"{request.Version}|{request.Nonce}|{ts}|{apiUrl}|{agentName}");
            var response = new DiscoveryResponseDto(
                request.Version,
                request.Nonce,
                ts,
                apiUrl,
                agentName,
                responseProof);

            try
            {
                var payload = JsonSerializer.SerializeToUtf8Bytes(response);
                await udpClient.SendAsync(payload, payload.Length, received.RemoteEndPoint);
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "Ошибка UDP-ответа discovery.");
            }
        }
    }

    private static bool IsTimestampValid(long timestampUnix)
    {
        var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        return Math.Abs(now - timestampUnix) <= ClockSkewSeconds;
    }

    private static bool FixedTimeEquals(string left, string right)
    {
        var leftBytes = Encoding.UTF8.GetBytes(left);
        var rightBytes = Encoding.UTF8.GetBytes(right);
        return CryptographicOperations.FixedTimeEquals(leftBytes, rightBytes);
    }

    private static string ComputeHmac(string key, string payload)
    {
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(key));
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
        return Convert.ToHexString(hash);
    }

    private static bool TryResolveServerAddressForClient(IPEndPoint clientEndPoint, out string host)
    {
        host = string.Empty;
        try
        {
            using var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            socket.Connect(clientEndPoint);
            var local = socket.LocalEndPoint as IPEndPoint;
            if (local is not null
                && local.Address.AddressFamily == AddressFamily.InterNetwork
                && !IPAddress.IsLoopback(local.Address))
            {
                host = local.Address.ToString();
                return true;
            }
        }
        catch
        {
            // Игнорируем и пробуем fallback ниже.
        }

        var fallback = Dns.GetHostEntry(Dns.GetHostName())
            .AddressList
            .FirstOrDefault(address => address.AddressFamily == AddressFamily.InterNetwork && !IPAddress.IsLoopback(address));
        if (fallback is null)
        {
            return false;
        }

        host = fallback.ToString();
        return true;
    }

    private sealed record DiscoveryRequestDto(
        int Version,
        string PairId,
        string Nonce,
        long TimestampUnix,
        string Proof);

    private sealed record DiscoveryResponseDto(
        int Version,
        string Nonce,
        long TimestampUnix,
        string ApiUrl,
        string AgentName,
        string Proof);
}

