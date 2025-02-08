using ClipboardMonitor.Core.Enums;
using ClipboardMonitor.Core.EventArguments;
using ClipboardMonitor.Core.Interfaces;
using System.Runtime.InteropServices;

namespace ClipboardMonitor.Core.ClipboardListenerImp
{
    public class WindowsClipboardListener : IClipboardListener
    {
        // Define a delegate matching the C++ callback function signature
        private delegate void ClipboardChangedCallback();

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void ClipboardChangedCallbackWithData([MarshalAs(UnmanagedType.LPStr)] string data, int type);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void ClipboardChangedCallbackWithImage(IntPtr data, UIntPtr length, int type);

        // Import SetClipboardChangedCallback function from the DLL
        [DllImport("ClipboardMonitor.Windows.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern void SetClipboardChangedCallback(ClipboardChangedCallback? callback);

        // Import StartClipboardListener function from the DLL
        [DllImport("ClipboardMonitor.Windows.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern void StartClipboardListener();

        [DllImport("ClipboardMonitor.Windows.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern void SetClipboardChangedCallbackWithData(ClipboardChangedCallbackWithData? callback);

        [DllImport("ClipboardMonitor.Windows.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern void SetClipboardChangedCallbackWithImage(ClipboardChangedCallbackWithImage? callback);

        private Task? _clipboardMonitorTask;
        private CancellationTokenSource? _clipboardMonitorTokenCts;
        private NotificationType _notificationType;
        private bool _callbacksSet;
        private ClipboardChangedCallbackWithData? _clipboardChangedCallbackWithData;
        private ClipboardChangedCallbackWithImage? _clipboardChangedCallbackWithImage;

        public event EventHandler<ClipboardChangedEventArgs>? ClipboardChanged;

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
            SetCallbacks();
        }

        public void Start()
        {
            if (IsMonitoring)
                return;

            if (!_callbacksSet)
                SetCallbacks();

            //StartClipboardListener(); - Running this on the main thread works fine, but locks the thread!

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

            RemoveCallbacks();
            IsMonitoring = false;
        }

        private async Task StartListener()
        {
            StartClipboardListener();
            await Task.CompletedTask;
        }

        private void SetCallbacks()
        {
            switch (_notificationType)
            {
                case NotificationType.ChangedWithData:
                    SetCallbacksWithData();
                    break;

                case NotificationType.ChangeNotificationOnly:
                    SetClipboardChangedCallback(OnClipboardChanged);
                    break;

                case NotificationType.All:
                    SetCallbacksWithData();
                    SetClipboardChangedCallback(OnClipboardChanged);
                    break;
            }

            _callbacksSet = true;
        }

        private void RemoveCallbacks()
        {
            SetClipboardChangedCallback(null);
            SetCallbacksWithData(true);

            _callbacksSet = false;
        }

        private void SetCallbacksWithData(bool unset = false)
        {
            if (unset)
            {
                SetClipboardChangedCallbackWithData(null);
                SetClipboardChangedCallbackWithImage(null);
                _clipboardChangedCallbackWithData = null;
                _clipboardChangedCallbackWithImage = null;
            }
            else
            {
                _clipboardChangedCallbackWithData = OnClipboardChangedWithData;
                SetClipboardChangedCallbackWithData(_clipboardChangedCallbackWithData);
                _clipboardChangedCallbackWithImage = OnClipboardChangedWithImage;  // Keep delegate alive
                SetClipboardChangedCallbackWithImage(_clipboardChangedCallbackWithImage);
            }
        }

        // Add additional methods as needed, depending on what you need from your C++ DLL
        // The callback function called when the clipboard changes
        private void OnClipboardChanged()
        {
            OnClipboardChanged(new ClipboardChangedEventArgs(ClipboardDataType.NONE));
        }

        private void OnClipboardChangedWithData(string data, int type)
        {
            var dataType = (ClipboardDataType)type;

            switch (dataType)
            {
                case ClipboardDataType.TEXT:
                    Console.WriteLine("Text copied: " + data); // For debug only
                    OnClipboardChanged(new ClipboardChangedEventArgs(data, null, ClipboardDataType.TEXT));
                    break;
                case ClipboardDataType.FILES:
                    Console.WriteLine("Files copied: " + data); // For debug only
                    OnClipboardChanged(new ClipboardChangedEventArgs(data, null, ClipboardDataType.FILES));
                    break;
                default:
                    Console.WriteLine("Unknown clipboard event");
                    break;
            }
        }

        private void OnClipboardChangedWithImage(IntPtr data, UIntPtr length, int type)
        {
            Console.WriteLine($"Received image data: Length={length}, Type={type}");
            var imageData = new byte[(int)length];
            Marshal.Copy(data, imageData, 0, imageData.Length);

            Console.WriteLine("Files copied: Image"); // For debug only
            OnClipboardChanged(new ClipboardChangedEventArgs(null, imageData, ClipboardDataType.IMAGE));
        }

        private void OnClipboardChanged(ClipboardChangedEventArgs e) => ClipboardChanged?.Invoke(this, e);
    }
}
