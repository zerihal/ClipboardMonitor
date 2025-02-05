using ClipboardMonitor.Core.Enums;

namespace ClipboardMonitor.Core.EventArguments
{
    public class ClipboardChangedEventArgs : EventArgs
    {
        public string? ClipboardText { get; }

        public byte[]? ClipboardImageData { get; }

        public bool IsClipboardTextFilesPath => DataType == ClipboardDataType.FILES;

        public bool IsNotificationOnly => DataType == ClipboardDataType.NONE;

        public ClipboardDataType DataType { get; }

        public IEnumerable<string>? ClipboardFiles
        {
            get
            {
                if (DataType == ClipboardDataType.FILES && !string.IsNullOrWhiteSpace(ClipboardText))
                    return ClipboardText.Split("\n");

                return null;
            }
        }

        public ClipboardChangedEventArgs(string? clipboardText, byte[]? clipboardImageData, ClipboardDataType type) : this(type)
        {
            ClipboardText = clipboardText;
            ClipboardImageData = clipboardImageData;
        }

        public ClipboardChangedEventArgs(ClipboardDataType type)
        {
            DataType = type;
        }
    }
}
