using System.Diagnostics;

namespace Server.Infrastructure.Execution;

/// <summary>
/// Активация схемы питания Windows через <c>powercfg /setactive</c>.
/// </summary>
public static class WindowsPowerPlanActivator
{
    /// <summary>
    /// Активирует схему питания по GUID.
    /// </summary>
    /// <param name="schemeGuid">GUID схемы.</param>
    /// <param name="cancellationToken">Токен отмены ожидания завершения процесса.</param>
    /// <returns>Краткое сообщение об успехе или детали ошибки.</returns>
    public static string Activate(string schemeGuid, CancellationToken cancellationToken = default)
    {
        if (!OperatingSystem.IsWindows())
        {
            throw new PlatformNotSupportedException("Смена схемы питания поддерживается только в Windows.");
        }

        if (string.IsNullOrWhiteSpace(schemeGuid) || !Guid.TryParse(schemeGuid, out _))
        {
            throw new ArgumentException("Некорректный GUID схемы питания.", nameof(schemeGuid));
        }

        var psi = new ProcessStartInfo
        {
            FileName = "powercfg.exe",
            Arguments = $"/setactive {schemeGuid}",
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        };

        using var process = Process.Start(psi);
        if (process is null)
        {
            throw new InvalidOperationException("Не удалось запустить powercfg.exe.");
        }

        // Ожидание с периодической проверкой отмены.
        while (!process.WaitForExit(500))
        {
            cancellationToken.ThrowIfCancellationRequested();
        }

        var stderr = process.StandardError.ReadToEnd();
        var stdout = process.StandardOutput.ReadToEnd();

        if (process.ExitCode != 0)
        {
            var detail = string.IsNullOrWhiteSpace(stderr) ? stdout : stderr;
            throw new InvalidOperationException(
                string.IsNullOrWhiteSpace(detail)
                    ? $"powercfg завершился с кодом {process.ExitCode}."
                    : detail.Trim());
        }

        return "Схема питания активирована.";
    }
}
