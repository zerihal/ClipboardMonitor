using ClipboardMonitor.Core.Enums;
using ClipboardMonitor.Core.EventArguments;
using ClipboardMonitor.Core.Interfaces;

namespace ClipboardMonitor.Core.ClipboardListenerImp
{
    public abstract class ClipboardListenerBase : IClipboardListener
    {
        protected Task? _clipboardMonitorTask;
        protected CancellationTokenSource? _clipboardMonitorTokenCts;
        protected NotificationType _notificationType;
        protected bool _callbacksSet;

        /// <inheritdoc/>
        public bool IsMonitoring { get; protected set; }

        /// <inheritdoc/>
        public event EventHandler<ClipboardChangedEventArgs>? ClipboardChanged;

        public virtual void SetNotificationType(NotificationType notificationType)
        {
            if (_notificationType == notificationType) return;

            _notificationType = notificationType;

            // Remove the callbacks first and then set to the corresponding value for the noticiation type
            RemoveCallbacks();

            // Now set the callbacks again using the new notification type
            SetCallbacks();
        }

        /// <inheritdoc/>
        public virtual void Start()
        {
            if (IsMonitoring) return;

            if (!_callbacksSet)
                SetCallbacks();

            _clipboardMonitorTokenCts = new CancellationTokenSource();
            _clipboardMonitorTask = Task.Run(StartListener, _clipboardMonitorTokenCts.Token);

            IsMonitoring = true;
        }

        /// <inheritdoc/>
        public virtual void Stop()
        {
            if (!IsMonitoring) return;

            RemoveCallbacks();
            StopListener();
            _clipboardMonitorTokenCts?.Cancel();
            _clipboardMonitorTokenCts = null;
            _clipboardMonitorTask = null;

            IsMonitoring = false;
        }

        /// <summary>
        /// Sets callbacks according as per notification type set.
        /// </summary>
        protected void SetCallbacks()
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

        /// <summary>
        /// Removes all callbacks.
        /// </summary>
        protected void RemoveCallbacks()
        {
            SetCallbacksNoData(true);
            SetCallbacksWithData(true);

            _callbacksSet = false;
        }

        /// <summary>
        /// Sets callbacks according as per notification type set.
        /// </summary>
        protected abstract void SetCallbacksNoData(bool unset = false);

        /// <summary>
        /// Removes all callbacks.
        /// </summary>
        protected abstract void SetCallbacksWithData(bool unset = false);

        /// <summary>
        /// Starts the listener in a separate thread so not to lock the UI / main thread.
        /// </summary>
        /// <remarks>
        /// Note: This method should be overriden as async void.
        /// </remarks>
        protected abstract void StartListener();

        /// <summary>
        /// Stops the listener and cleans up resources.
        /// </summary>
        protected abstract void StopListener();

        /// <summary>
        /// Clipboard changed event handler.
        /// </summary>
        /// <param name="e">Windows clipboard changed event arguments.</param>
        protected void OnClipboardChanged(ClipboardChangedEventArgs e) => ClipboardChanged?.Invoke(this, e);
    }
}
