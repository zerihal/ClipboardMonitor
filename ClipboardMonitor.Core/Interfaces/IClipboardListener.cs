using ClipboardMonitor.Core.Enums;

namespace ClipboardMonitor.Core.Interfaces
{
    public interface IClipboardListener
    {
        void SetNotificationType(NotificationType notificationType);

        void Start();

        void Stop();
    }
}
