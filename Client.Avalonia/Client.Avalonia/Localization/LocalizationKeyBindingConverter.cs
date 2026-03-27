using System.Globalization;
using Avalonia.Data.Converters;

namespace Client.Avalonia.Localization;

/// <summary>
/// Контекст параметров для конвертера локализации.
/// </summary>
public sealed class LocalizationConverterContext
{
    /// <summary>
    /// Индекс значения ключа из привязки.
    /// </summary>
    public int? KeyIndex { get; init; }

    /// <summary>
    /// Индекс значения аргументов из привязки.
    /// </summary>
    public int? ArgsIndex { get; init; }

    /// <summary>
    /// Индекс значения формата из привязки.
    /// </summary>
    public int? StringFormatIndex { get; init; }

    /// <summary>
    /// Статический ключ перевода.
    /// </summary>
    public string? StaticKey { get; init; }

    /// <summary>
    /// Статические ключи аргументов.
    /// </summary>
    public IReadOnlyList<string> StaticArgsKeys { get; init; } = [];

    /// <summary>
    /// Статический ключ шаблона форматирования.
    /// </summary>
    public string? StaticStringFormatKey { get; init; }
}

/// <summary>
/// Конвертер локализации для сценариев Key/KeyBinding c аргументами и итоговым форматированием.
/// </summary>
public sealed class LocalizationKeyBindingConverter : IMultiValueConverter
{
    /// <summary>
    /// Singleton-экземпляр конвертера.
    /// </summary>
    public static LocalizationKeyBindingConverter Instance { get; } = new();

    private LocalizationKeyBindingConverter()
    {
    }

    /// <inheritdoc />
    public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (parameter is not LocalizationConverterContext context)
        {
            return string.Empty;
        }

        var key = GetBoundString(values, context.KeyIndex) ?? context.StaticKey;
        if (string.IsNullOrWhiteSpace(key))
        {
            return string.Empty;
        }

        var source = LocalizationBindingSource.Instance;
        var currentCulture = source.CurrentCulture;
        var translated = source.Translate(key);

        var args = BuildArguments(values, context, source);
        if (args.Count > 0)
        {
            translated = string.Format(currentCulture, translated, args.ToArray());
        }

        var stringFormat = GetBoundString(values, context.StringFormatIndex);
        if (string.IsNullOrWhiteSpace(stringFormat) && !string.IsNullOrWhiteSpace(context.StaticStringFormatKey))
        {
            stringFormat = source.Translate(context.StaticStringFormatKey!);
        }

        if (!string.IsNullOrWhiteSpace(stringFormat))
        {
            return string.Format(currentCulture, stringFormat, translated);
        }

        return translated;
    }

    private static string? GetBoundString(IList<object?> values, int? index)
    {
        if (index == null || index < 0 || index >= values.Count)
        {
            return null;
        }

        return values[index.Value]?.ToString();
    }

    private static List<object?> BuildArguments(
        IList<object?> values,
        LocalizationConverterContext context,
        LocalizationBindingSource source)
    {
        var args = new List<object?>();

        foreach (var key in context.StaticArgsKeys)
        {
            args.Add(source.Translate(key));
        }

        if (context.ArgsIndex == null || context.ArgsIndex < 0 || context.ArgsIndex >= values.Count)
        {
            return args;
        }

        var boundArgsValue = values[context.ArgsIndex.Value];
        if (boundArgsValue == null)
        {
            return args;
        }

        if (boundArgsValue is string)
        {
            args.Add(boundArgsValue);
            return args;
        }

        if (boundArgsValue is IEnumerable<object?> objectEnumerable)
        {
            args.AddRange(objectEnumerable);
            return args;
        }

        if (boundArgsValue is System.Collections.IEnumerable enumerable)
        {
            foreach (var item in enumerable)
            {
                args.Add(item);
            }

            return args;
        }

        args.Add(boundArgsValue);
        return args;
    }
}
