using System.Globalization;
using System.Resources;
using System.Reflection;

namespace Client.Avalonia.Localization;

/// <summary>
/// Реализация локализации на базе встроенных RESX-словарей.
/// </summary>
public sealed class LocalizationService : ILocalizationService
{
    private const string ResourceNamespacePrefix = "Client.Avalonia.Resources.Localization.";
    private static readonly string[] ResourceNames = ["CommonStrings", "CommandStrings", "HistoryStrings", "RedmineStrings"];

    private readonly Dictionary<string, ResourceManager> _resourceManagers = new(StringComparer.Ordinal);
    private CultureInfo _currentCulture;

    /// <summary>
    /// Создаёт сервис локализации с культурой по умолчанию из <see cref="CultureInfo.CurrentUICulture"/>.
    /// </summary>
    public LocalizationService()
    {
        var assembly = Assembly.GetExecutingAssembly();
        foreach (var resourceName in ResourceNames)
        {
            _resourceManagers[resourceName] =
                new ResourceManager($"{ResourceNamespacePrefix}{resourceName}", assembly);
        }

        _currentCulture = CultureInfo.CurrentUICulture;
    }

    /// <inheritdoc />
    public event EventHandler? CultureChanged;

    /// <inheritdoc />
    public CultureInfo CurrentCulture
    {
        get => _currentCulture;
        set
        {
            ArgumentNullException.ThrowIfNull(value);
            if (Equals(_currentCulture.Name, value.Name))
            {
                return;
            }

            _currentCulture = value;
            CultureInfo.DefaultThreadCurrentUICulture = value;
            CultureInfo.DefaultThreadCurrentCulture = value;
            CultureChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    /// <inheritdoc />
    public string GetString(string resourceKey)
    {
        ArgumentException.ThrowIfNullOrEmpty(resourceKey);
        var (resourceName, keyName) = ParseCompositeResourceKey(resourceKey);
        if (!_resourceManagers.TryGetValue(resourceName, out var resourceManager))
        {
            return resourceKey;
        }

        var text = resourceManager.GetString(keyName, _currentCulture);
        if (!string.IsNullOrEmpty(text))
        {
            return text;
        }

        text = resourceManager.GetString(keyName, CultureInfo.InvariantCulture);
        return !string.IsNullOrEmpty(text) ? text : resourceKey;
    }

    /// <inheritdoc />
    public string GetString(string resourceKey, params object?[] formatArgs)
    {
        var template = GetString(resourceKey);
        if (formatArgs is { Length: > 0 })
        {
            return string.Format(_currentCulture, template, formatArgs);
        }

        return template;
    }

    private static (string ResourceName, string KeyName) ParseCompositeResourceKey(string compositeKey)
    {
        var separatorIndex = compositeKey.IndexOf('.');
        if (separatorIndex <= 0 || separatorIndex >= compositeKey.Length - 1)
        {
            return (string.Empty, compositeKey);
        }

        return (
            compositeKey[..separatorIndex],
            compositeKey[(separatorIndex + 1)..]);
    }
}
