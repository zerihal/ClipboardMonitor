using ClipboardMonitor.Core.Enums;
using ClipboardMonitor.Core.EventArguments;
using ClipboardMonitor.Core.Interfaces;
using System.Drawing;
using System.Runtime.InteropServices;

namespace ClipboardMonitor.Core.ClipboardListenerImp
{
    public class WindowsClipboardListener : IWindowsClipboardListener
    {
        // Define a delegate matching the C++ callback function signature
        private delegate void ClipboardChangedCallback();

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void ClipboardChangedCallbackWithData([MarshalAs(UnmanagedType.LPStr)] string data, int type);

        // Import SetClipboardChangedCallback function from the DLL
        [DllImport("ClipboardMonitor.Windows.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern void SetClipboardChangedCallback(ClipboardChangedCallback? callback);

        // Import StartClipboardListener function from the DLL
        [DllImport("ClipboardMonitor.Windows.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern void StartClipboardListener();

        [DllImport("ClipboardMonitor.Windows.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern void SetClipboardChangedCallbackWithData(ClipboardChangedCallbackWithData? callback);

        private Task? _clipboardMonitorTask;
        private CancellationTokenSource? _clipboardMonitorTokenCts;
        private NotificationType _notificationType;
        private bool _callbacksSet;
        private ClipboardChangedCallback? _clipboardChangedCallbackNoData;
        private ClipboardChangedCallbackWithData? _clipboardChangedCallbackWithData;
        private string? _lastStringData;

        public event EventHandler<WinClipboardChangedEventArgs>? ClipboardChanged;

        public bool IsMonitoring { get; private set; }

        public WindowsClipboardListener()
        {
            SetNotificationType(NotificationType.ChangedWithData);
        }

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

        private void RemoveCallbacks()
        {
            SetCallbacksNoData(true);
            SetCallbacksWithData(true);

            _callbacksSet = false;
        }

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

        private async void StartListener()
        {
            StartClipboardListener();
            await Task.CompletedTask;
        }

        // Add additional methods as needed, depending on what you need from your C++ DLL
        // The callback function called when the clipboard changes
        private void OnClipboardChangedNoData()
        {
            Console.WriteLine("Clipboard content changed"); // Debug only
            OnClipboardChanged(new WinClipboardChangedEventArgs(ClipboardDataType.NONE));
        }

        private void OnClipboardChangedWithData(string data, int type)
        {
            var dataType = (ClipboardDataType)type;

            switch (dataType)
            {
                case ClipboardDataType.TEXT:
                    if (IsNewClipboardData(data))
                    {
                        Console.WriteLine("Text copied: " + data); // Debug only
                        OnClipboardChanged(new WinClipboardChangedEventArgs(data, ClipboardDataType.TEXT));
                    }
                    break;

                case ClipboardDataType.FILES:
                    if (IsNewClipboardData(data))
                    {
                        Console.WriteLine("Files copied: " + data); // Debug only
                        OnClipboardChanged(new WinClipboardChangedEventArgs(data, ClipboardDataType.FILES));
                    }
                    break;

                case ClipboardDataType.IMAGE:
                    if (GetBitmapFromClipboard() is Bitmap bitmap)
                    {
                        Console.WriteLine("Image copied"); // Debug only
                        OnClipboardChanged(new WinClipboardChangedEventArgs(bitmap, ClipboardDataType.IMAGE));
                    }
                    break;

                default:
                    Console.WriteLine("Unknown clipboard event"); // Debug only
                    break;
            }
        }

        public bool IsNewClipboardData(string data)
        {
            if (string.IsNullOrEmpty(data) || data == _lastStringData)
                return false;

            _lastStringData = data;
            return true;
        }

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

        private void OnClipboardChanged(WinClipboardChangedEventArgs e) => ClipboardChanged?.Invoke(this, e);
    }
}
