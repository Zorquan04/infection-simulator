using System.Text.Json;
using InfectionSimulator.Models;

namespace InfectionSimulator.Services;

public static class SnapshotManager
{
    public static void SaveSnapshot(string path, IEnumerable<PersonMemento> memos)
    {
        var options = new JsonSerializerOptions { WriteIndented = true };
        File.WriteAllText(path, JsonSerializer.Serialize(memos, options));
    }

    public static List<PersonMemento> LoadSnapshot(string path)
    {
        var txt = File.ReadAllText(path);
        return JsonSerializer.Deserialize<List<PersonMemento>>(txt) ?? new List<PersonMemento>();
    }
}