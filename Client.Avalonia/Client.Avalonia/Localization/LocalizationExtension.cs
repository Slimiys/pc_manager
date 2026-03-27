using Avalonia.Data;
using Avalonia.Markup.Xaml;
using Avalonia.Markup.Xaml.MarkupExtensions;

namespace Client.Avalonia.Localization;

/// <summary>
/// Расширение разметки для локализации.
/// </summary>
/// <remarks>
/// Поддерживает следующие сценарии:
/// 1) <c>{localization:Localization Key=CommonStrings.AppTitle}</c>
/// 2) <c>{localization:Localization KeyBinding={Binding StatusKey}}</c>
/// 3) <c>{localization:Localization Key=CommonStrings.StatusProcessing ArgsKey=CommandStrings.CommandDisplay_GetUptime}</c>
/// 4) <c>{localization:Localization KeyBinding={Binding StatusKey} StringFormatBinding={Binding StatusWrapperFormat}}</c>
/// </remarks>
public sealed class LocalizationExtension : MarkupExtension
{
    /// <summary>
    /// Создаёт расширение локализации без параметров.
    /// </summary>
    public LocalizationExtension()
    {
    }

    /// <summary>
    /// Создаёт расширение локализации с позиционным ключом.
    /// Позволяет писать: <c>{localization:Localization Some.Key}</c>.
    /// </summary>
    /// <param name="key">Ключ ресурса локализации.</param>
    public LocalizationExtension(string key)
    {
        Key = key;
    }

    /// <summary>
    /// Статический ключ ресурса для прямого перевода.
    /// </summary>
    public string? Key { get; set; }

    /// <summary>
    /// Привязка, которая возвращает ключ ресурса для перевода.
    /// </summary>
    public IBinding? KeyBinding { get; set; }

    /// <summary>
    /// Привязка для аргументов форматирования локализованной строки.
    /// Может возвращать одно значение, массив или коллекцию.
    /// </summary>
    public IBinding? ArgsBinding { get; set; }

    /// <summary>
    /// Статический ключ одного аргумента.
    /// </summary>
    public string? ArgsKey { get; set; }

    /// <summary>
    /// Статические ключи аргументов, разделённые <c>;</c> или <c>,</c>.
    /// </summary>
    public string? ArgsKeys { get; set; }

    /// <summary>
    /// Привязка шаблона форматирования, применяемого к уже переведённой строке.
    /// </summary>
    public IBinding? StringFormatBinding { get; set; }

    /// <summary>
    /// Ключ ресурса шаблона форматирования, применяемого к уже переведённой строке.
    /// </summary>
    public string? StringFormatKey { get; set; }

    /// <inheritdoc />
    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        ValidateConfiguration();

        var source = LocalizationBindingSource.Instance;
        var bindings = new List<IBinding>();

        int? keyIndex = null;
        if (KeyBinding != null)
        {
            keyIndex = bindings.Count;
            bindings.Add(KeyBinding);
        }

        int? argsIndex = null;
        if (ArgsBinding != null)
        {
            argsIndex = bindings.Count;
            bindings.Add(ArgsBinding);
        }

        int? stringFormatIndex = null;
        if (StringFormatBinding != null)
        {
            stringFormatIndex = bindings.Count;
            bindings.Add(StringFormatBinding);
        }

        bindings.Add(new Binding(nameof(LocalizationBindingSource.Version)) { Source = source });

        return new MultiBinding
        {
            Bindings = bindings,
            Converter = LocalizationKeyBindingConverter.Instance,
            ConverterParameter = new LocalizationConverterContext
            {
                KeyIndex = keyIndex,
                ArgsIndex = argsIndex,
                StringFormatIndex = stringFormatIndex,
                StaticKey = Key,
                StaticArgsKeys = ParseStaticArgsKeys(ArgsKey, ArgsKeys),
                StaticStringFormatKey = StringFormatKey
            }
        };
    }

    private void ValidateConfiguration()
    {
        if (!string.IsNullOrWhiteSpace(Key) && KeyBinding != null)
        {
            // Ошибка конфигурации должна проявляться на этапе компиляции XAML.
            throw new XamlLoadException(
                "LocalizationExtension: нельзя одновременно задавать Key и KeyBinding.");
        }

        if (!string.IsNullOrWhiteSpace(StringFormatKey) && StringFormatBinding != null)
        {
            // Ошибка конфигурации должна проявляться на этапе компиляции XAML.
            throw new XamlLoadException(
                "LocalizationExtension: нельзя одновременно задавать StringFormatKey и StringFormatBinding.");
        }
    }

    private static IReadOnlyList<string> ParseStaticArgsKeys(string? argsKey, string? argsKeys)
    {
        var result = new List<string>();
        if (!string.IsNullOrWhiteSpace(argsKey))
        {
            result.Add(argsKey.Trim());
        }

        if (string.IsNullOrWhiteSpace(argsKeys))
        {
            return result;
        }

        var split = argsKeys.Split([',', ';'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        result.AddRange(split);
        return result;
    }
}
