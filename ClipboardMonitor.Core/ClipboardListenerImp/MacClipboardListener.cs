using ClipboardMonitor.Core.Enums;
using ClipboardMonitor.Core.Interfaces;

namespace ClipboardMonitor.Core.ClipboardListenerImp
{
    public class MacClipboardListener : IMacClipboardListener
    {
        /// <inheritdoc/>
        public bool IsMonitoring { get; private set; }

        public MacClipboardListener() 
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
