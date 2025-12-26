namespace ClipboardMonitor.Core.Enums
{
    /// <summary>
    /// Clipboard data types.
    /// </summary>
    /// <remarks>
    /// Note: This enum matches the data types used in native assemblies for clipboard monitoring.
    /// </remarks>
    public enum ClipboardDataType
    {
        NONE,
        TEXT,
        FILES,
        IMAGE,
        OTHER,
        CLEARED
    }
}
