using NAutoSuite.Core.Data;
using System.Text.Json;

namespace PickAndPlace.Backend.Machine;

/// <summary>
/// Machine-specific data for Pick and Place
/// Common settings (MachineId, CycleTimeout, etc.) are already in MachineData base class
/// This class ONLY defines Pick and Place specific settings
/// </summary>
public class PickAndPlaceData : MachineData
{
    // Positions
    public PositionData PickPosition { get; set; } = new(100, 50, 50);
    public PositionData PlacePosition { get; set; } = new(200, 150, 50);
    public PositionData HomePosition { get; set; } = new(0, 0, 0);
    public PositionData SafeZPosition { get; set; } = new(0, 0, 100); // Safe height for moving

    // Speeds
    public SpeedData HighSpeed { get; set; } = new(100, 500, 500);    // Fast move
    public SpeedData LowSpeed { get; set; } = new(10, 100, 100);      // Precise move
    public SpeedData PickSpeed { get; set; } = new(20, 200, 200);     // Pick operation
    public SpeedData PlaceSpeed { get; set; } = new(20, 200, 200);    // Place operation

    // Pick and Place specific settings
    public double PickHeight { get; set; } = 10.0;              // mm - Z height when picking
    public double PlaceHeight { get; set; } = 10.0;             // mm - Z height when placing
    public double VacuumOnDelay { get; set; } = 0.5;            // seconds - wait after vacuum ON
    public double VacuumOffDelay { get; set; } = 0.3;           // seconds - wait after vacuum OFF
    public double BlowOffDuration { get; set; } = 0.2;          // seconds - blow off time
    public bool CheckPartPresence { get; set; } = true;         // Check sensor before pick
    public bool CheckVacuumSensor { get; set; } = true;         // Verify vacuum after pick

    // Constructor with default values
    public PickAndPlaceData()
    {
        // Set common settings specific to this machine type
        Common.MachineId = "PAP001";
        Common.MachineName = "Pick and Place Machine";
        Common.CycleTimeout = 30.0;  // 30 seconds per cycle
    }

    /// <summary>
    /// Load data from JSON file
    /// </summary>
    public override void Load(string filePath)
    {
        try
        {
            if (!File.Exists(filePath))
            {
                // Create default file if not exists
                Save(filePath);
                return;
            }

            var json = File.ReadAllText(filePath);
            var data = JsonSerializer.Deserialize<PickAndPlaceData>(json);

            if (data != null)
            {
                // Copy all properties
                Common = data.Common;
                PickPosition = data.PickPosition;
                PlacePosition = data.PlacePosition;
                HomePosition = data.HomePosition;
                SafeZPosition = data.SafeZPosition;
                HighSpeed = data.HighSpeed;
                LowSpeed = data.LowSpeed;
                PickSpeed = data.PickSpeed;
                PlaceSpeed = data.PlaceSpeed;
                PickHeight = data.PickHeight;
                PlaceHeight = data.PlaceHeight;
                VacuumOnDelay = data.VacuumOnDelay;
                VacuumOffDelay = data.VacuumOffDelay;
                BlowOffDuration = data.BlowOffDuration;
                CheckPartPresence = data.CheckPartPresence;
                CheckVacuumSensor = data.CheckVacuumSensor;
            }
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to load data from {filePath}", ex);
        }
    }

    /// <summary>
    /// Save data to JSON file
    /// </summary>
    public override void Save(string filePath)
    {
        try
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true
            };

            var json = JsonSerializer.Serialize(this, options);
            File.WriteAllText(filePath, json);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to save data to {filePath}", ex);
        }
    }
}
