using ClipboardMonitor.Core.EventArguments;

namespace ClipboardMonitor.Core.Interfaces
{
    public interface IWindowsClipboardListener : IClipboardListener
    {
        event EventHandler<WinClipboardChangedEventArgs>? ClipboardChanged;
    }
}
