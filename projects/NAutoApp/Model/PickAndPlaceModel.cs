using NAutoSuite.Core.Model;

namespace NAutoApp.Model;

/// <summary>
/// Model data for Pick and Place machine
/// Contains teaching positions, speeds, and process parameters
/// </summary>
public class NAutoAppModel : ModelBase
{
    #region Teaching Positions (using base TeachPositions dictionary)

    // Predefined position keys for easy access
    public const string POS_HOME = "Home";
    public const string POS_PICK = "Pick";
    public const string POS_PLACE = "Place";
    public const string POS_SAFE_Z = "SafeZ";

    #endregion

    #region Speed Settings (using base SpeedSettings dictionary)

    // Predefined speed keys
    public const string SPEED_MOVE = "MoveSpeed";
    public const string SPEED_APPROACH = "ApproachSpeed";
    public const string SPEED_PICK = "PickSpeed";
    public const string SPEED_PLACE = "PlaceSpeed";

    #endregion

    #region Delay Settings (using base DelaySettings dictionary)

    // Predefined delay keys
    public const string DELAY_VACUUM_ON = "VacuumOnDelay";
    public const string DELAY_VACUUM_OFF = "VacuumOffDelay";
    public const string DELAY_PICK = "PickDelay";
    public const string DELAY_PLACE = "PlaceDelay";

    #endregion

    #region Pick and Place Specific Parameters

    /// <summary>
    /// Vacuum level threshold (0-100%)
    /// </summary>
    public int VacuumThreshold { get; set; } = 70;

    /// <summary>
    /// Number of retry attempts for pick operation
    /// </summary>
    public int PickRetryCount { get; set; } = 3;

    /// <summary>
    /// Enable/disable part presence check
    /// </summary>
    public bool EnablePartCheck { get; set; } = true;

    /// <summary>
    /// Cycle time target in seconds
    /// </summary>
    public double TargetCycleTime { get; set; } = 5.0;

    #endregion

    public NAutoAppModel()
    {
        // Initialize default teaching positions
        TeachPositions[POS_HOME] = new TeachPosition(POS_HOME, x: 0, y: 0, z: 0);
        TeachPositions[POS_PICK] = new TeachPosition(POS_PICK, x: 100, y: 50, z: 10);
        TeachPositions[POS_PLACE] = new TeachPosition(POS_PLACE, x: 200, y: 50, z: 10);
        TeachPositions[POS_SAFE_Z] = new TeachPosition(POS_SAFE_Z, z: 50);

        // Initialize default speeds (mm/s)
        SpeedSettings[SPEED_MOVE] = 100;
        SpeedSettings[SPEED_APPROACH] = 50;
        SpeedSettings[SPEED_PICK] = 20;
        SpeedSettings[SPEED_PLACE] = 20;

        // Initialize default delays (ms)
        DelaySettings[DELAY_VACUUM_ON] = 200;
        DelaySettings[DELAY_VACUUM_OFF] = 100;
        DelaySettings[DELAY_PICK] = 300;
        DelaySettings[DELAY_PLACE] = 300;
    }

    #region Helper Properties for easy access

    public TeachPosition HomePosition
    {
        get => TeachPositions.GetValueOrDefault(POS_HOME) ?? new TeachPosition(POS_HOME);
        set => TeachPositions[POS_HOME] = value;
    }

    public TeachPosition PickPosition
    {
        get => TeachPositions.GetValueOrDefault(POS_PICK) ?? new TeachPosition(POS_PICK);
        set => TeachPositions[POS_PICK] = value;
    }

    public TeachPosition PlacePosition
    {
        get => TeachPositions.GetValueOrDefault(POS_PLACE) ?? new TeachPosition(POS_PLACE);
        set => TeachPositions[POS_PLACE] = value;
    }

    public double MoveSpeed
    {
        get => SpeedSettings.GetValueOrDefault(SPEED_MOVE, 100);
        set => SpeedSettings[SPEED_MOVE] = value;
    }

    public double ApproachSpeed
    {
        get => SpeedSettings.GetValueOrDefault(SPEED_APPROACH, 50);
        set => SpeedSettings[SPEED_APPROACH] = value;
    }

    public int VacuumOnDelay
    {
        get => DelaySettings.GetValueOrDefault(DELAY_VACUUM_ON, 200);
        set => DelaySettings[DELAY_VACUUM_ON] = value;
    }

    public int VacuumOffDelay
    {
        get => DelaySettings.GetValueOrDefault(DELAY_VACUUM_OFF, 100);
        set => DelaySettings[DELAY_VACUUM_OFF] = value;
    }

    #endregion

    public override List<string> Validate()
    {
        var errors = base.Validate();

        // Validate positions exist
        if (!TeachPositions.ContainsKey(POS_PICK))
        {
            errors.Add("Pick position is not defined");
        }

        if (!TeachPositions.ContainsKey(POS_PLACE))
        {
            errors.Add("Place position is not defined");
        }

        // Validate speeds
        if (MoveSpeed <= 0)
        {
            errors.Add("Move speed must be greater than 0");
        }

        // Validate vacuum threshold
        if (VacuumThreshold < 0 || VacuumThreshold > 100)
        {
            errors.Add("Vacuum threshold must be between 0 and 100");
        }

        return errors;
    }
}

