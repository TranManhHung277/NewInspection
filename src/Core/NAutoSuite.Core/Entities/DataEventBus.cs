namespace NAutoSuite.Core.Entities;

public sealed class DataEventBus
{
    public event EventHandler<DataChangedEventArgs>? DataChanged;

    public void PublishDataChanged(DataChangedEventArgs args)
    {
        DataChanged?.Invoke(this, args);
    }
}

public sealed class DataChangedEventArgs : EventArgs
{
    public DataChangedEventArgs(string systemName, double value, string dataType, DateTime timestampUtc)
    {
        SystemName = systemName;
        Value = value;
        DataType = dataType;
        TimestampUtc = timestampUtc;
    }

    public string SystemName { get; }
    public double Value { get; }
    public string DataType { get; }
    public DateTime TimestampUtc { get; }
}
