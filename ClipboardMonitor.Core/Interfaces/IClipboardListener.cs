using ClipboardMonitor.Core.Enums;
using ClipboardMonitor.Core.EventArguments;

namespace ClipboardMonitor.Core.Interfaces
{
    public interface IClipboardListener
    {
        void SetNotificationType(NotificationType notificationType);

        void Start();

        void Stop();
    }
}
