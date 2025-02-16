using ClipboardMonitor.Core.Enums;
using ClipboardMonitor.Core.EventArguments;
using ClipboardMonitor.Core.Interfaces;
using System.Runtime.InteropServices;

namespace ClipboardMonitor.Core.ClipboardListenerImp
{
    public class LinuxClipboardListener : ClipboardListenerBase, ILinuxClipboardListener
    {
        private const string LibName = "libClipboardMonitor.Linux.so";

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void ClipboardChangedCallback();

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void ClipboardChangedCallbackWithData(IntPtr data, int type);

        [DllImport(LibName, EntryPoint = "StartClipboardListener", CallingConvention = CallingConvention.Cdecl)]
        private static extern void StartClipboardListener();

        [DllImport(LibName, EntryPoint = "SetClipboardChangedCallback", CallingConvention = CallingConvention.Cdecl)]
        private static extern void SetClipboardChangedCallback(ClipboardChangedCallback? callback);

        [DllImport(LibName, EntryPoint = "SetClipboardChangedCallbackWithData", CallingConvention = CallingConvention.Cdecl)]
        private static extern void SetClipboardChangedCallbackWithData(ClipboardChangedCallbackWithData? callback);

        private ClipboardChangedCallback? _clipboardChangedCallbackNoData;
        private ClipboardChangedCallbackWithData? _clipboardChangedCallbackWithData;

        public event EventHandler<LinuxClipboardChangedEventArgs>? ClipboardChanged;

        public LinuxClipboardListener() 
        {
            SetDllResolver(); // Ensure library is correctly loaded
            SetNotificationType(NotificationType.ChangedWithData);
        }

        protected override void SetCallbacksNoData(bool unset = false)
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

        protected override void SetCallbacksWithData(bool unset = false)
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

        protected override async void StartListener()
        {
            StartClipboardListener();
            await Task.CompletedTask;
        }

        protected override void OnClipboardChangedNoData()
        {
            Console.WriteLine("ClipboardChanged");
            OnClipboardChanged(new LinuxClipboardChangedEventArgs(ClipboardDataType.NONE));
        }

        private void OnClipboardChangedWithData(IntPtr data, int type)
        {
            Console.WriteLine("ClipboardChanged with data");
            OnClipboardChanged(new LinuxClipboardChangedEventArgs(ClipboardDataType.OTHER));
        }

        private void SetDllResolver()
        {
            NativeLibrary.SetDllImportResolver(typeof(LinuxClipboardListener).Assembly, (libraryName, assembly, searchPath) =>
            {
                if (libraryName == LibName)
                {
                    string libPath = Path.Combine(AppContext.BaseDirectory, LibName);
                    Console.WriteLine($"Loading Clipboard Library from: {libPath}");
                    return NativeLibrary.Load(libPath);
                }
                return IntPtr.Zero;
            });
        }

        /// <summary>
        /// Clipboard changed event handler.
        /// </summary>
        /// <param name="e">Windows clipboard changed event arguments.</param>
        private void OnClipboardChanged(LinuxClipboardChangedEventArgs e) => ClipboardChanged?.Invoke(this, e);
    }
}
