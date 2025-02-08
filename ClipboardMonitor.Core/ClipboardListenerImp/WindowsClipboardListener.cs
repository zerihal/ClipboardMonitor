using ClipboardMonitor.Core.Enums;
using ClipboardMonitor.Core.EventArguments;
using ClipboardMonitor.Core.Interfaces;
using System.Drawing;
using System.Runtime.InteropServices;

namespace ClipboardMonitor.Core.ClipboardListenerImp
{
    public class WindowsClipboardListener : IClipboardListener
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
            SetClipboardChangedCallback(null);
            SetClipboardChangedCallbackWithData(null);

            switch (_notificationType)
            {
                case NotificationType.ChangedWithData:
                    SetClipboardChangedCallbackWithData(OnClipboardChangedWithData);
                    break;

                case NotificationType.ChangeNotificationOnly:
                    SetClipboardChangedCallback(OnClipboardChanged);
                    break;

                case NotificationType.All:
                    SetClipboardChangedCallbackWithData(OnClipboardChangedWithData);
                    SetClipboardChangedCallback(OnClipboardChanged);
                    break;
            }
        }

        public void Start()
        {
            if (IsMonitoring)
                return;

            _clipboardMonitorTokenCts = new CancellationTokenSource();
            _clipboardMonitorTask = Task.Run(StartListener, _clipboardMonitorTokenCts.Token);

            IsMonitoring = true;
        }

        public void Stop()
        {
            if (!IsMonitoring)
                return;

            _clipboardMonitorTokenCts?.Cancel();
            _clipboardMonitorTokenCts = null;
            _clipboardMonitorTask = null;

            IsMonitoring = false;
        }

        private async void StartListener()
        {
            StartClipboardListener();
            await Task.CompletedTask;
        }

        // Add additional methods as needed, depending on what you need from your C++ DLL
        // The callback function called when the clipboard changes
        private void OnClipboardChanged()
        {
            Console.WriteLine("Clipboard content changed (from .NET)!");

            // This is to get the clipboard content and then fire an event to notify any listeners of the
            // content that has been added
        }

        private void OnClipboardChangedWithData(string data, int type)
        {
            var dataType = (ClipboardDataType)type;

            switch (dataType)
            {
                case ClipboardDataType.TEXT:
                    Console.WriteLine("Text copied: " + data);
                    OnClipboardChanged(new WinClipboardChangedEventArgs(data, ClipboardDataType.TEXT));
                    break;
                case ClipboardDataType.FILES:
                    Console.WriteLine("Files copied: " + data);
                    OnClipboardChanged(new WinClipboardChangedEventArgs(data, ClipboardDataType.FILES));
                    break;
                case ClipboardDataType.IMG_BITMAP:
                case ClipboardDataType.IMG_DIB:
                    Console.WriteLine("Image copied");
                    OnClipboardChanged(new WinClipboardChangedEventArgs(GetBitmapFromClipboard(), ClipboardDataType.IMAGE));
                    break;
                default:
                    Console.WriteLine("Unknown clipboard event");
                    break;
            }
        }

        private static Bitmap? GetBitmapFromClipboard()
        {
            Bitmap? bitmap = null;

            if (NativeMethods.IsClipboardFormatAvailable(NativeMethods.CF_BITMAP))
            {
                if (NativeMethods.OpenClipboard(IntPtr.Zero))
                {
                    try
                    {
                        IntPtr hBitmap = NativeMethods.GetClipboardData(NativeMethods.CF_BITMAP);
                        if (hBitmap != IntPtr.Zero)
                        {
                            // Create .NET Bitmap from GDI bitmap handle
                            #pragma warning disable CA1416 // Validate platform compatibility - this is Windows implementation only
                            bitmap = Image.FromHbitmap(hBitmap);
                            #pragma warning restore CA1416 // Validate platform compatibility

                            // Ensure the GDI object is released
                            NativeMethods.DeleteObject(hBitmap);
                        }
                    }
                    finally
                    {
                        NativeMethods.CloseClipboard();
                    }
                }
            }
            return bitmap;
        }

        private void OnClipboardChanged(WinClipboardChangedEventArgs e) => ClipboardChanged?.Invoke(this, e);
    }
}
