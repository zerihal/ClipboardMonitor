using ClipboardMonitor.Core.ClipboardObjects;
using ClipboardMonitor.Core.Enums;
using ClipboardMonitor.Core.EventArguments;
using ClipboardMonitor.Core.Interfaces;
using System.Runtime.InteropServices;
using System.Text;

namespace ClipboardMonitor.Core.ClipboardListenerImp
{
    public class LinuxClipboardListener : ClipboardListenerBase, ILinuxClipboardListener
    {
        // Delegate matching the Linux callback function signature for clipboard changed
        private delegate void ClipboardChangedCallback();

        // Delegate matching the Linux callback function signature for clipboard changed with data
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void ClipboardChangedCallbackWithData(IntPtr data, int dataSize, int type);

        // Import StartClipboardListener function from the .so
        [DllImport("libClipboardMonitor.Linux.so", CallingConvention = CallingConvention.Cdecl)]
        private static extern void StartClipboardListener();

        // Import StopClipboardListener function from the .so
        [DllImport("libClipboardMonitor.Linux.so", CallingConvention = CallingConvention.Cdecl)]
        private static extern void StopClipboardListener();

        // Import SetClipboardChangedCallback function from the .so
        [DllImport("libClipboardMonitor.Linux.so", CallingConvention = CallingConvention.Cdecl)]
        private static extern void SetClipboardChangedCallback(ClipboardChangedCallback? callback);

        // Import SetClipboardChangedCallbackWithData function from the .so
        [DllImport("libClipboardMonitor.Linux.so", CallingConvention = CallingConvention.Cdecl)]
        private static extern void SetClipboardChangedCallbackWithData(ClipboardChangedCallbackWithData? callback);

        private ClipboardChangedCallback? _clipboardChangedCallbackNoData;
        private ClipboardChangedCallbackWithData? _clipboardChangedCallbackWithData;

        /// <summary>
        /// Creates a new instance of the Linux clipboard listener with default notification type of ChangedWithData.
        /// </summary>
        public LinuxClipboardListener() 
        {
            SetNotificationType(NotificationType.ChangedWithData);
        }

        /// <inheritdoc/>
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

        /// <inheritdoc/>
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

        /// <inheritdoc/>
        protected override async void StartListener()
        {
            StartClipboardListener();
            await Task.CompletedTask;
        }

        /// <inheritdoc/>
        protected override void StopListener() => StopClipboardListener();

        /// <summary>
        /// Clipboard changed callback with no data.
        /// </summary>
        private void OnClipboardChangedNoData()
        {
            Console.WriteLine("Clipboard content changed"); // Debug only - to be removed
            OnClipboardChanged(new ClipboardChangedEventArgs(ClipboardDataType.NONE));
        }

        /// <summary>
        /// Clipboard changed callback with data.
        /// </summary>
        /// <param name="data">String data, such as clipboard text or files.</param>
        /// <param name="size">Size of data.</param>
        /// <param name="type">Type of data (i.e. text, files, or image).</param>
        private void OnClipboardChangedWithData(IntPtr data, int size, int type)
        {
            var dataType = (ClipboardDataType)type;

            switch (dataType)
            {
                case ClipboardDataType.TEXT:
                case ClipboardDataType.FILES:
                    {
                        // Copy bytes and convert to string
                        byte[] buffer = new byte[size];
                        Marshal.Copy(data, buffer, 0, size);
                        string text = Encoding.UTF8.GetString(buffer);

                        if (dataType == ClipboardDataType.TEXT)
                        {
                            Console.WriteLine("Text copied: " + text); // Debug
                            OnClipboardChanged(new ClipboardChangedEventArgs(text, ClipboardDataType.TEXT));
                        }
                        else
                        {
                            Console.WriteLine("Files copied: " + text); // Debug
                            OnClipboardChanged(new ClipboardChangedEventArgs(text, ClipboardDataType.FILES));
                        }
                        break;
                    }

                case ClipboardDataType.IMAGE:
                    {
                        // Copy raw bytes
                        byte[] buffer = new byte[size];
                        Marshal.Copy(data, buffer, 0, size);
                        Console.WriteLine("Image copied"); // Debug
                        Console.WriteLine("Image data size: " + size); // Debug

                        // Wrap in your ClipboardImage class
                        var image = ClipboardImage.FromClipboardData(buffer, size);
                        OnClipboardChanged(new ClipboardChangedEventArgs(image, ClipboardDataType.IMAGE));
                        break;
                    }

                default:
                    Console.WriteLine("Unknown clipboard event"); // Debug
                    break;
            }
        }
    }
}
