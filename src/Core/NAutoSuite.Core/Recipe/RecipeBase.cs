using System.Text.Json;

namespace NAutoSuite.Core.Recipe;

/// <summary>
/// Base class for machine recipes
/// </summary>
public abstract class RecipeBase
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? ModifiedAt { get; set; }
    public string Version { get; set; } = "1.0";

    protected RecipeBase()
    {
        CreatedAt = DateTime.Now;
    }

    /// <summary>
    /// Validate recipe parameters
    /// </summary>
    public abstract bool Validate();

    /// <summary>
    /// Save recipe to file
    /// </summary>
    public async Task SaveAsync(string filePath)
    {
        var json = JsonSerializer.Serialize(this, new JsonSerializerOptions
        {
            WriteIndented = true
        });
        await File.WriteAllTextAsync(filePath, json);
        ModifiedAt = DateTime.Now;
    }

    /// <summary>
    /// Load recipe from file
    /// </summary>
    public static async Task<T?> LoadAsync<T>(string filePath) where T : RecipeBase
    {
        var json = await File.ReadAllTextAsync(filePath);
        return JsonSerializer.Deserialize<T>(json);
    }
}
