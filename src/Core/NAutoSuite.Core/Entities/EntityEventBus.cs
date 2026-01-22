namespace NAutoSuite.Core.Entities;

public sealed class EntityEventBus
{
    public event EventHandler<EntityStateChangedEventArgs>? StateChanged;

    public void PublishStateChanged(EntityStateChangedEventArgs args)
    {
        StateChanged?.Invoke(this, args);
    }
}

public sealed class EntityStateChangedEventArgs : EventArgs
{
    public EntityStateChangedEventArgs(string systemName, bool value, EntityDirection direction, DateTime timestampUtc)
    {
        SystemName = systemName;
        Value = value;
        Direction = direction;
        TimestampUtc = timestampUtc;
    }

    public string SystemName { get; }

    public bool Value { get; }

    public EntityDirection Direction { get; }

    public DateTime TimestampUtc { get; }
}
