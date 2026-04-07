using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Client.Avalonia.Services;

/// <summary>
/// UDP-клиент обнаружения API в локальной сети.
/// </summary>
public sealed class LanDiscoveryClient
{
    private const int MaxDatagramSize = 4096;
    private const int Version = 1;
    private const int ClockSkewSeconds = 30;
    private const string PlaceholderKeyPrefix = "CHANGE_ME";

    /// <summary>
    /// Пытается найти адрес API в локальной сети с HMAC-валидацией ответа.
    /// </summary>
    /// <param name="settings">Настройки API и discovery.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Найденный URL API или null.</returns>
    public async Task<string?> TryDiscoverBaseUrlAsync(ApiSettings settings, CancellationToken cancellationToken)
    {
        if (!settings.EnableLanDiscovery
            || string.IsNullOrWhiteSpace(settings.DiscoverySharedKey)
            || settings.DiscoverySharedKey.StartsWith(PlaceholderKeyPrefix, StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        var nonce = Convert.ToHexString(RandomNumberGenerator.GetBytes(16));
        var ts = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var proof = ComputeHmac(
            settings.DiscoverySharedKey,
            $"{Version}|{settings.DiscoveryPairId}|{nonce}|{ts}");

        var request = new DiscoveryRequestDto(Version, settings.DiscoveryPairId, nonce, ts, proof);
        var payload = JsonSerializer.SerializeToUtf8Bytes(request);

        using var udpClient = new UdpClient(AddressFamily.InterNetwork);
        udpClient.EnableBroadcast = true;
        udpClient.Client.ReceiveTimeout = settings.DiscoveryTimeoutMs;

        var endpoint = new IPEndPoint(IPAddress.Broadcast, settings.DiscoveryUdpPort);
        await udpClient.SendAsync(payload, payload.Length, endpoint);

        using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeoutCts.CancelAfter(TimeSpan.FromMilliseconds(settings.DiscoveryTimeoutMs));

        while (!timeoutCts.IsCancellationRequested)
        {
            UdpReceiveResult received;
            try
            {
                received = await udpClient.ReceiveAsync(timeoutCts.Token);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch
            {
                break;
            }

            if (received.Buffer.Length == 0 || received.Buffer.Length > MaxDatagramSize)
            {
                continue;
            }

            DiscoveryResponseDto? response;
            try
            {
                response = JsonSerializer.Deserialize<DiscoveryResponseDto>(received.Buffer);
            }
            catch
            {
                continue;
            }

            if (response is null || response.Version != Version || !string.Equals(response.Nonce, nonce, StringComparison.Ordinal))
            {
                continue;
            }

            if (!IsTimestampValid(response.TimestampUnix))
            {
                continue;
            }

            var expectedProof = ComputeHmac(
                settings.DiscoverySharedKey,
                $"{response.Version}|{response.Nonce}|{response.TimestampUnix}|{response.ApiUrl}|{response.AgentName}");
            if (!FixedTimeEquals(expectedProof, response.Proof))
            {
                continue;
            }

            if (!Uri.TryCreate(response.ApiUrl, UriKind.Absolute, out var uri))
            {
                continue;
            }

            return uri.ToString().TrimEnd('/');
        }

        return null;
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

