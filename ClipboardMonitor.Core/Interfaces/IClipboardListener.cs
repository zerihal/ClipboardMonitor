using ClipboardMonitor.Core.Enums;
using ClipboardMonitor.Core.EventArguments;

namespace ClipboardMonitor.Core.Interfaces
{
    public interface IClipboardListener
    {
        /// <summary>
        /// Clipboard changed event.
        /// </summary>
        event EventHandler<ClipboardChangedEventArgs>? ClipboardChanged;

        /// <summary>
        /// Flag to indicate whether clipboard is listener is currently active.
        /// </summary>
        bool IsMonitoring { get; }

        /// <summary>
        /// Sets notification type - clipboard changed with or without data.
        /// </summary>
        /// <param name="notificationType">Notification type.</param>
        void SetNotificationType(NotificationType notificationType);

        /// <summary>
        /// Starts the clipboard monitor for notification type selected.
        /// </summary>
        void Start();

        /// <summary>
        /// Stops the clipboard monitor, cleaning up callbacks and resources.
        /// </summary>
        void Stop();
    }
}
