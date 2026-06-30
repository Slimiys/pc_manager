using System.Text.Json;
using Server.Application.Contracts;

namespace Server.Infrastructure.Redmine;

/// <summary>
/// Файловое хранилище состояния seen issue → status id.
/// </summary>
public sealed class FileRedmineSeenStateStore : IRedmineSeenStateStore
{
    private readonly string _stateFilePath;
    private readonly SemaphoreSlim _sync = new(1, 1);

    /// <summary>
    /// Создаёт хранилище состояния Redmine.
    /// </summary>
    /// <param name="stateFilePath">Абсолютный путь к JSON-файлу.</param>
    public FileRedmineSeenStateStore(string stateFilePath)
    {
        _stateFilePath = stateFilePath;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyDictionary<string, int>> LoadAsync(CancellationToken cancellationToken)
    {
        await _sync.WaitAsync(cancellationToken);
        try
        {
            if (!File.Exists(_stateFilePath))
            {
                return new Dictionary<string, int>(StringComparer.Ordinal);
            }

            await using var stream = File.OpenRead(_stateFilePath);
            var data = await JsonSerializer.DeserializeAsync<Dictionary<string, int>>(stream, cancellationToken: cancellationToken);
            return data ?? new Dictionary<string, int>(StringComparer.Ordinal);
        }
        finally
        {
            _sync.Release();
        }
    }

    /// <inheritdoc />
    public async Task SaveAsync(IReadOnlyDictionary<string, int> seenState, CancellationToken cancellationToken)
    {
        await _sync.WaitAsync(cancellationToken);
        try
        {
            var directory = Path.GetDirectoryName(_stateFilePath);
            if (!string.IsNullOrWhiteSpace(directory))
            {
                Directory.CreateDirectory(directory);
            }

            await using var stream = File.Create(_stateFilePath);
            await JsonSerializer.SerializeAsync(stream, seenState, cancellationToken: cancellationToken);
        }
        finally
        {
            _sync.Release();
        }
    }
}
