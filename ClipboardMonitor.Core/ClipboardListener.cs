using ClipboardMonitor.Core.Interfaces;
using System.Runtime.InteropServices;

namespace ClipboardMonitor.Core
{
    public class ClipboardListener : IClipboardListener
    {
        // Define a delegate matching the C++ callback function signature
        private delegate void ClipboardChangedCallback();

        // Import SetClipboardChangedCallback function from the DLL
        [DllImport("ClipboardMonitor.Windows.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern void SetClipboardChangedCallback(ClipboardChangedCallback callback);

        // Import StartClipboardListener function from the DLL
        [DllImport("ClipboardMonitor.Windows.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern void StartClipboardListener();

        private Task? _clipboardMonitorTask;
        private CancellationTokenSource? _clipboardMonitorTokenCts;

        public bool IsMonitoring { get; private set; }

        public ClipboardListener()
        {
            SetClipboardChangedCallback(OnClipboardChanged);
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
    }
}
