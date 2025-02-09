using ClipboardMonitor.Core.EventArguments;

namespace ClipboardMonitor.Core.Interfaces
{
    public interface IWindowsClipboardListener : IClipboardListener
    {
        /// <summary>
        /// Clipboard changed event.
        /// </summary>
        event EventHandler<WinClipboardChangedEventArgs>? ClipboardChanged;
    }
}
