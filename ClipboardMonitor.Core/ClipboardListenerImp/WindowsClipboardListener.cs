using ClipboardMonitor.Core.Enums;
using ClipboardMonitor.Core.EventArguments;
using ClipboardMonitor.Core.Interfaces;
using System.Drawing;
using System.Runtime.InteropServices;

namespace ClipboardMonitor.Core.ClipboardListenerImp
{
    public class WindowsClipboardListener : IWindowsClipboardListener
    {
        // Delegate matching the ClipboardMonitor.Windows callback function signature for clipboard changed
        private delegate void ClipboardChangedCallback();

        // Delegate matching the ClipboardMonitor.Windows callback function signature for clipboard changed with data
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void ClipboardChangedCallbackWithData([MarshalAs(UnmanagedType.LPStr)] string data, int type);

        // Import SetClipboardChangedCallback function from the DLL
        [DllImport("ClipboardMonitor.Windows.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern void SetClipboardChangedCallback(ClipboardChangedCallback? callback);

        // Import StartClipboardListener function from the DLL
        [DllImport("ClipboardMonitor.Windows.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern void StartClipboardListener();

        // Import SetClipboardChangedCallbackWithData function from the DLL
        [DllImport("ClipboardMonitor.Windows.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern void SetClipboardChangedCallbackWithData(ClipboardChangedCallbackWithData? callback);

        private Task? _clipboardMonitorTask;
        private CancellationTokenSource? _clipboardMonitorTokenCts;
        private NotificationType _notificationType;
        private bool _callbacksSet;
        private ClipboardChangedCallback? _clipboardChangedCallbackNoData;
        private ClipboardChangedCallbackWithData? _clipboardChangedCallbackWithData;
        private string? _lastStringData;

        /// <inheritdoc/>
        public event EventHandler<WinClipboardChangedEventArgs>? ClipboardChanged;

        /// <inheritdoc/>
        public bool IsMonitoring { get; private set; }

        public WindowsClipboardListener()
        {
            SetNotificationType(NotificationType.ChangedWithData);
        }

        /// <inheritdoc/>
        public void SetNotificationType(NotificationType notificationType)
        {
            if (_notificationType == notificationType) return;

            _notificationType = notificationType;

            // Remove the callbacks first and then set to the corresponding value for the noticiation type
            RemoveCallbacks();

            // Now set the callbacks again using the new notification type
            SetCallbacks();
        }

        /// <inheritdoc/>
        public void Start()
        {
            if (IsMonitoring) return;

            if (!_callbacksSet)
                SetCallbacks();

            _clipboardMonitorTokenCts = new CancellationTokenSource();
            _clipboardMonitorTask = Task.Run(StartListener, _clipboardMonitorTokenCts.Token);

            IsMonitoring = true;
        }

        /// <inheritdoc/>
        public void Stop()
        {
            if (!IsMonitoring) return;

            RemoveCallbacks();
            _clipboardMonitorTokenCts?.Cancel();
            _clipboardMonitorTokenCts = null;
            _clipboardMonitorTask = null;

            IsMonitoring = false;
        }

        /// <summary>
        /// Sets callbacks according as per notification type set.
        /// </summary>
        private void SetCallbacks()
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
        private void RemoveCallbacks()
        {
            SetCallbacksNoData(true);
            SetCallbacksWithData(true);

            _callbacksSet = false;
        }

        /// <summary>
        /// Sets or unsets callbacks for clipboard changed with no data
        /// </summary>
        /// <param name="unset">True to unset, or false (default) to set.</param>
        private void SetCallbacksNoData(bool unset = false)
        {
            if (unset)
            {
                SetClipboardChangedCallback(null);
                _clipboardChangedCallbackNoData = null;
            }
            else
            {
                _clipboardChangedCallbackNoData = OnClipboardChangedNoData;
                SetClipboardChangedCallback(_clipboardChangedCallbackNoData);
            }
        }

        /// <summary>
        /// Sets or unsets callbacks for clipboard changed with data
        /// </summary>
        /// <param name="unset">True to unset, or false (default) to set.</param>
        private void SetCallbacksWithData(bool unset = false)
        {
            if (unset)
            {
                SetClipboardChangedCallbackWithData(null);
                _clipboardChangedCallbackWithData = null;
            }
            else
            {
                _clipboardChangedCallbackWithData = OnClipboardChangedWithData;
                SetClipboardChangedCallbackWithData(_clipboardChangedCallbackWithData);
            }
        }

        /// <summary>
        /// Starts the listener in a separate thread so not to lock the UI / main thread.
        /// </summary>
        private async void StartListener()
        {
            StartClipboardListener();
            await Task.CompletedTask;
        }

        /// <summary>
        /// Clipboard changed callback with no data.
        /// </summary>
        private void OnClipboardChangedNoData()
        {
            Console.WriteLine("Clipboard content changed"); // Debug only - to be removed
            OnClipboardChanged(new WinClipboardChangedEventArgs(ClipboardDataType.NONE));
        }

        /// <summary>
        /// Clipboard changed callback with data.
        /// </summary>
        /// <param name="data">String data, such as clipboard text or files.</param>
        /// <param name="type">Type of data (i.e. text, files, or image).</param>
        private void OnClipboardChangedWithData(string data, int type)
        {
            var dataType = (ClipboardDataType)type;

            switch (dataType)
            {
                case ClipboardDataType.TEXT:
                    if (IsNewClipboardData(data))
                    {
                        Console.WriteLine("Text copied: " + data); // Debug only - to be removed
                        OnClipboardChanged(new WinClipboardChangedEventArgs(data, ClipboardDataType.TEXT));
                    }
                    break;

                case ClipboardDataType.FILES:
                    if (IsNewClipboardData(data))
                    {
                        Console.WriteLine("Files copied: " + data); // Debug only - to be removed
                        OnClipboardChanged(new WinClipboardChangedEventArgs(data, ClipboardDataType.FILES));
                    }
                    break;

                case ClipboardDataType.IMAGE:
                    if (GetBitmapFromClipboard() is Bitmap bitmap)
                    {
                        Console.WriteLine("Image copied"); // Debug only - to be removed
                        OnClipboardChanged(new WinClipboardChangedEventArgs(bitmap, ClipboardDataType.IMAGE));
                    }
                    break;

                default:
                    Console.WriteLine("Unknown clipboard event"); // Debug only - to be removed
                    break;
            }
        }

        /// <summary>
        /// Checks whether the clipboard data is new or repeat of the last addition.
        /// </summary>
        /// <param name="data">Clipboard data.</param>
        /// <returns>True if the clipboard data is new from last sequence, otherwise false for duplicate.</returns>
        public bool IsNewClipboardData(string data)
        {
            if (string.IsNullOrEmpty(data) || data == _lastStringData)
                return false;

            _lastStringData = data;
            return true;
        }

        /// <summary>
        /// Uses Windows native methods to get image data from the clipboard.
        /// </summary>
        /// <returns>Image (bitmap) if available.</returns>
        private Bitmap? GetBitmapFromClipboard()
        {
            Bitmap? bitmap = null;

            if (WindowsNativeMethods.IsClipboardFormatAvailable(WindowsNativeMethods.CF_BITMAP))
            {
                if (WindowsNativeMethods.OpenClipboard(IntPtr.Zero))
                {
                    try
                    {
                        IntPtr hBitmap = WindowsNativeMethods.GetClipboardData(WindowsNativeMethods.CF_BITMAP);
                        if (hBitmap != IntPtr.Zero)
                        {
                            // Create .NET Bitmap from GDI bitmap handle
                            #pragma warning disable CA1416 // Validate platform compatibility - this is Windows implementation only
                            bitmap = Image.FromHbitmap(hBitmap);
                            #pragma warning restore CA1416 // Validate platform compatibility

                            // Ensure the GDI object is released
                            WindowsNativeMethods.DeleteObject(hBitmap);
                        }
                    }
                    finally
                    {
                        WindowsNativeMethods.CloseClipboard();
                    }
                }
            }
            return bitmap;
        }

        /// <summary>
        /// Clipboard changed event handler.
        /// </summary>
        /// <param name="e">Windows clipboard changed event arguments.</param>
        private void OnClipboardChanged(WinClipboardChangedEventArgs e) => ClipboardChanged?.Invoke(this, e);
    }
}
