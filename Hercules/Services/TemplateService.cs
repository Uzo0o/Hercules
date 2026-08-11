using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Hercules.Models.Templates;

namespace Hercules.Services;

public class TemplateService
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        WriteIndented = true
    };

    public async Task SaveAsync(string filePath, HerculesTemplate template)
    {
        template.SavedAtUtc = DateTime.UtcNow.ToString("o");
        string json = JsonSerializer.Serialize(template, SerializerOptions);
        await File.WriteAllTextAsync(filePath, json);
    }

    // Returns null (rather than throwing) on a missing/corrupt file - callers
    // decide how to surface that to the user instead of the app crashing on
    // a template someone hand-edited badly or picked the wrong file.
    public async Task<HerculesTemplate?> LoadAsync(string filePath)
    {
        try
        {
            string json = await File.ReadAllTextAsync(filePath);
            return JsonSerializer.Deserialize<HerculesTemplate>(json, SerializerOptions);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[TEMPLATE ERROR] Failed to load '{filePath}': {ex.Message}");
            return null;
        }
    }
}