using Microsoft.Extensions.Localization;
using System.Collections.Generic;

/// This helper class allows unit and integration tests to construct services that depend on
/// <see cref="IStringLocalizer{T}"/> without needing actual resource (.resx) files or localization setup.
/// When accessed, each localized string simply returns the key itself as the value,
/// making it ideal for scenarios where localization is not under test.
public sealed class NullStringLocalizer<T> : IStringLocalizer<T>
{
    public LocalizedString this[string name] => new(name, name);
    public LocalizedString this[string name, params object[] arguments] => new(name, string.Format(name, arguments));
    public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures) => System.Array.Empty<LocalizedString>();
}
