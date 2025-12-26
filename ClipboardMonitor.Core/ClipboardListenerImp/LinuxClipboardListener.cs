using ClipboardMonitor.Core.ClipboardObjects;
using ClipboardMonitor.Core.Enums;
using ClipboardMonitor.Core.EventArguments;
using ClipboardMonitor.Core.Interfaces;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace ClipboardMonitor.Core.ClipboardListenerImp
{
    public class LinuxClipboardListener : ClipboardListenerBase, ILinuxClipboardListener
    {
        private const string NativeDllName = "libClipboardMonitor.Linux.so";
        private const CallingConvention CallConv = CallingConvention.Cdecl;

        // Delegate matching the Linux callback function signature for clipboard changed
        private delegate void ClipboardChangedCallback();

        // Delegate matching the Linux callback function signature for clipboard changed with data
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void ClipboardChangedCallbackWithData(IntPtr data, int dataSize, int type);

        // Import StartClipboardListener function from the .so
        [DllImport(NativeDllName, CallingConvention = CallConv)]
        private static extern void StartClipboardListener();

        // Import StopClipboardListener function from the .so
        [DllImport(NativeDllName, CallingConvention = CallConv)]
        private static extern void StopClipboardListener();

        // Import SetClipboardChangedCallback function from the .so
        [DllImport(NativeDllName, CallingConvention = CallConv)]
        private static extern void SetClipboardChangedCallback(ClipboardChangedCallback? callback);

        // Import SetClipboardChangedCallbackWithData function from the .so
        [DllImport(NativeDllName, CallingConvention = CallConv)]
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
        public override bool ClearClipboardContent()
        {
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = "xclip",
                    Arguments = "-selection clipboard",
                    RedirectStandardInput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                // Set DISPLAY if needed
                if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("DISPLAY")))
                    psi.Environment["DISPLAY"] = ":0";

                using var process = Process.Start(psi);
                process!.StandardInput.Write("");   // empty clipboard
                process.StandardInput.Flush();
                process.StandardInput.Close();
                process.WaitForExit();

                if (process.ExitCode == 0)
                {
                    OnClipboardChanged(new ClipboardChangedEventArgs(ClipboardDataType.CLEARED));
                    return true;
                }
                else
                {
                    // Exit code should usually be 0 (error with this will normally be caught below), but just in case
                    // not 0, we treat as failure.
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed to clear clipboard: " + ex.Message);
                return false;
            }
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
                    Console.WriteLine("Unknown clipboard event or no data"); // Debug
                    break;
            }
        }
    }
}
