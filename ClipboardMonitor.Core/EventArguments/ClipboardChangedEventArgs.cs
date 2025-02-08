using ClipboardMonitor.Core.Enums;

namespace ClipboardMonitor.Core.EventArguments
{
    public class ClipboardChangedEventArgs : EventArgs
    {
        public string? ClipboardText { get; }

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

        public ClipboardChangedEventArgs(string clipboardText, ClipboardDataType type) : this(type)
        {
            ClipboardText = clipboardText;
        }

        public ClipboardChangedEventArgs(ClipboardDataType type)
        {
            DataType = type;
        }
    }
}
