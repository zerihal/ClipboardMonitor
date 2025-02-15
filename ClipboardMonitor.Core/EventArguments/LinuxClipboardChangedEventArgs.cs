using ClipboardMonitor.Core.Enums;

namespace ClipboardMonitor.Core.EventArguments
{
    public class LinuxClipboardChangedEventArgs : ClipboardChangedEventArgs
    {
        public LinuxClipboardChangedEventArgs(ClipboardDataType type) : base(type)
        {
        }

        public LinuxClipboardChangedEventArgs(string clipboardText, ClipboardDataType type) : base(clipboardText, type)
        {
        }
    }
}
