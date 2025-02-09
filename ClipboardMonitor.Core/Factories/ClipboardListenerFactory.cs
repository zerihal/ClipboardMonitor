using ClipboardMonitor.Core.ClipboardListenerImp;
using ClipboardMonitor.Core.Interfaces;
using System.Runtime.InteropServices;

namespace ClipboardMonitor.Core.Factories
{
    public static class ClipboardListenerFactory
    {
        public static IClipboardListener CreateClipboardListener()
        {
            if (OperatingSystem.IsWindows())
            {
                return new WindowsClipboardListener();
            }
            else if (OperatingSystem.IsLinux())
            {
                return new LinuxClipboardListener();
            }
            else if (OperatingSystem.IsMacOS())
            {
                return new MacClipboardListener();
            }
            else
            {
                throw new PlatformNotSupportedException("Clipboard listener not supported on this platform.");
            }
        }

        public static T CreateClipboardListener<T>() where T : IClipboardListener
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) && typeof(T) == typeof(IWindowsClipboardListener))
            {
                return (T)(IClipboardListener)new WindowsClipboardListener();
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) && typeof(T) == typeof(ILinuxClipboardListener))
            {
                return (T)(IClipboardListener)new LinuxClipboardListener();
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX) && typeof(T) == typeof(IMacClipboardListener))
            {
                return (T)(IClipboardListener)new MacClipboardListener();
            }
            else
            {
                throw new NotSupportedException("Unsupported platform or interface requested.");
            }
        }
    }
}
