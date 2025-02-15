using ClipboardMonitor.Core.Enums;
using ClipboardMonitor.Core.Interfaces;

namespace ClipboardMonitor.Core.ClipboardListenerImp
{
    public abstract class ClipboardListenerBase : IClipboardListener
    {
        protected Task? _clipboardMonitorTask;
        protected CancellationTokenSource? _clipboardMonitorTokenCts;
        protected NotificationType _notificationType;
        protected bool _callbacksSet;
        private string? _lastStringData;

        public bool IsMonitoring { get; protected set; }

        public void SetNotificationType(NotificationType notificationType)
        {
            if (_notificationType == notificationType) return;

            _notificationType = notificationType;

            // Remove the callbacks first and then set to the corresponding value for the noticiation type
            RemoveCallbacks();

            // Now set the callbacks again using the new notification type
            SetCallbacks();
        }

        public void Start()
        {
            if (IsMonitoring) return;

            if (!_callbacksSet)
                SetCallbacks();

            _clipboardMonitorTokenCts = new CancellationTokenSource();
            _clipboardMonitorTask = Task.Run(StartListener, _clipboardMonitorTokenCts.Token);

            IsMonitoring = true;
        }

        public void Stop()
        {
            if (!IsMonitoring) return;

            RemoveCallbacks();
            _clipboardMonitorTokenCts?.Cancel();
            _clipboardMonitorTokenCts = null;
            _clipboardMonitorTask = null;

            IsMonitoring = false;
        }

        protected abstract void SetCallbacksNoData(bool unset = false);

        protected abstract void SetCallbacksWithData(bool unset = false);

        protected virtual void SetCallbacks()
        {
            switch (_notificationType)
            {
                case NotificationType.ChangedWithData:
                    SetCallbacksWithData();
                    break;

                case NotificationType.ChangeNotificationOnly:
                    SetCallbacksNoData();
                    break;

                case NotificationType.All:
                    SetCallbacksNoData();
                    SetCallbacksWithData();
                    break;
            }

            _callbacksSet = true;
        }

        protected virtual void RemoveCallbacks()
        {
            SetCallbacksNoData(true);
            SetCallbacksWithData(true);

            _callbacksSet = false;
        }

        protected abstract void StartListener();

        protected abstract void OnClipboardChangedNoData();

        /// <summary>
        /// Checks whether the clipboard data is new or repeat of the last addition.
        /// </summary>
        /// <param name="data">Clipboard data.</param>
        /// <returns>True if the clipboard data is new from last sequence, otherwise false for duplicate.</returns>
        protected bool IsNewClipboardData(string data)
        {
            if (string.IsNullOrEmpty(data) || data == _lastStringData)
                return false;

            _lastStringData = data;
            return true;
        }
    }
}
