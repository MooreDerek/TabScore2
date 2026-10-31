using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;

namespace TabScore2.Tests.Support;

internal sealed class TestSession : ISession
{
    private readonly Dictionary<string, byte[]> values = [];

    public bool IsAvailable => true;
    public string Id { get; } = Guid.NewGuid().ToString();
    public IEnumerable<string> Keys => values.Keys;

    public void Clear() => values.Clear();

    public Task CommitAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;

    public Task LoadAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;

    public void Remove(string key) => values.Remove(key);

    public void Set(string key, byte[] value) => values[key] = value;

    public bool TryGetValue(string key, out byte[] value)
    {
        if (values.TryGetValue(key, out byte[]? storedValue))
        {
            value = storedValue;
            return true;
        }

        value = [];
        return false;
    }
}

internal sealed class TestStringLocalizer<T> : IStringLocalizer<T>
{
    public LocalizedString this[string name] => new(name, name);

    public LocalizedString this[string name, params object[] arguments] =>
        new(name, string.Format(name, arguments));

    public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures) => [];
}

internal sealed class TestExternalNamesDatabase : TabScore2.DataServices.IExternalNamesDatabase
{
    public string GetExternalPlayerName(int playerNumber) => $"Player {playerNumber}";
}
