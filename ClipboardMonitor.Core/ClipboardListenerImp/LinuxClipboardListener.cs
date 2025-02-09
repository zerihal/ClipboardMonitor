using ClipboardMonitor.Core.Enums;
using ClipboardMonitor.Core.Interfaces;

namespace ClipboardMonitor.Core.ClipboardListenerImp
{
    public class LinuxClipboardListener : ILinuxClipboardListener
    {
        /// <inheritdoc/>
        public bool IsMonitoring { get; private set; }

        public LinuxClipboardListener() 
        {
            // Not implented yet
        }

        /// <inheritdoc/>
        public void SetNotificationType(NotificationType notificationType)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public void Start()
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public void Stop()
        {
            throw new NotImplementedException();
        }
    }
}
