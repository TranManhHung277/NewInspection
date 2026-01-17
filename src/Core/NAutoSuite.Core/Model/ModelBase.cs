using System.Text.Json;
using System.Text.Json.Serialization;

namespace NAutoSuite.Core.Model;

/// <summary>
/// Base class for machine models (teaching data, positions, speeds, etc.)
/// Models contain all the parameters needed to run a specific product/pattern
/// </summary>
public abstract class ModelBase
{
    /// <summary>
    /// Unique identifier for the model
    /// </summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Display name of the model
    /// </summary>
    public string Name { get; set; } = "New Model";

    /// <summary>
    /// Description of the model
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Version of the model
    /// </summary>
    public string Version { get; set; } = "1.0";

    /// <summary>
    /// Author who created/modified the model
    /// </summary>
    public string Author { get; set; } = Environment.UserName;

    /// <summary>
    /// Date when the model was created
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Date when the model was last modified
    /// </summary>
    public DateTime? ModifiedAt { get; set; }

    /// <summary>
    /// Whether this model is the default model
    /// </summary>
    public bool IsDefault { get; set; }

    /// <summary>
    /// Common teaching positions (can be extended in derived classes)
    /// Key: position name, Value: position data
    /// </summary>
    public Dictionary<string, TeachPosition> TeachPositions { get; set; } = new();

    /// <summary>
    /// Common speed settings
    /// Key: speed name, Value: speed value (mm/s or deg/s)
    /// </summary>
    public Dictionary<string, double> SpeedSettings { get; set; } = new();

    /// <summary>
    /// Common delay/timing settings (in milliseconds)
    /// Key: delay name, Value: delay value
    /// </summary>
    public Dictionary<string, int> DelaySettings { get; set; } = new();

    /// <summary>
    /// Additional custom parameters (flexible key-value storage)
    /// </summary>
    public Dictionary<string, object> CustomParameters { get; set; } = new();

    protected ModelBase()
    {
        CreatedAt = DateTime.Now;
    }

    /// <summary>
    /// Validate the model parameters
    /// </summary>
    /// <returns>List of validation errors, empty if valid</returns>
    public virtual List<string> Validate()
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(Name))
        {
            errors.Add("Model name is required");
        }

        return errors;
    }

    /// <summary>
    /// Check if the model is valid
    /// </summary>
    public bool IsValid => Validate().Count == 0;

    /// <summary>
    /// Mark the model as modified
    /// </summary>
    public void MarkModified()
    {
        ModifiedAt = DateTime.Now;
    }

    /// <summary>
    /// Clone the model with a new name
    /// </summary>
    public virtual ModelBase Clone(string newName)
    {
        var json = JsonSerializer.Serialize(this, GetType(), JsonOptions);
        var clone = (ModelBase)JsonSerializer.Deserialize(json, GetType(), JsonOptions)!;
        clone.Id = Guid.NewGuid().ToString();
        clone.Name = newName;
        clone.CreatedAt = DateTime.Now;
        clone.ModifiedAt = null;
        clone.IsDefault = false;
        return clone;
    }

    protected static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };
}

/// <summary>
/// Represents a taught position with optional axis values
/// </summary>
public class TeachPosition
{
    /// <summary>
    /// Position name/label
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// X axis position (mm)
    /// </summary>
    public double? X { get; set; }

    /// <summary>
    /// Y axis position (mm)
    /// </summary>
    public double? Y { get; set; }

    /// <summary>
    /// Z axis position (mm)
    /// </summary>
    public double? Z { get; set; }

    /// <summary>
    /// R (rotation) axis position (degrees)
    /// </summary>
    public double? R { get; set; }

    /// <summary>
    /// Additional axis positions if needed
    /// Key: axis name, Value: position
    /// </summary>
    public Dictionary<string, double> AdditionalAxes { get; set; } = new();

    /// <summary>
    /// Notes about this position
    /// </summary>
    public string? Notes { get; set; }

    public TeachPosition() { }

    public TeachPosition(string name, double? x = null, double? y = null, double? z = null, double? r = null)
    {
        Name = name;
        X = x;
        Y = y;
        Z = z;
        R = r;
    }

    /// <summary>
    /// Get position as array [X, Y, Z, R]
    /// </summary>
    public double?[] ToArray() => new[] { X, Y, Z, R };

    public override string ToString()
    {
        var parts = new List<string>();
        if (X.HasValue) parts.Add($"X:{X:F2}");
        if (Y.HasValue) parts.Add($"Y:{Y:F2}");
        if (Z.HasValue) parts.Add($"Z:{Z:F2}");
        if (R.HasValue) parts.Add($"R:{R:F2}");
        return $"{Name}: {string.Join(", ", parts)}";
    }
}
