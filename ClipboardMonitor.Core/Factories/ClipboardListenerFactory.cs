using ClipboardMonitor.Core.ClipboardListenerImp;
using ClipboardMonitor.Core.Interfaces;
using System.Runtime.InteropServices;

namespace ClipboardMonitor.Core.Factories
{
    public static class ClipboardListenerFactory
    {
        /// <summary>
        /// Creates an IClipboardListener implementation appropriate for the current platform.
        /// </summary>
        /// <returns>Implementation of IWindowsClipboardListener, ILinuxClipboardListener, or IMacClipboardListener depending on platform.</returns>
        /// <exception cref="PlatformNotSupportedException">Unsupported platform exception.</exception>
        public static IClipboardListener CreateClipboardListener()
        {
#if NET5_0_OR_GREATER
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
#else
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return new WindowsClipboardListener();
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                return new LinuxClipboardListener();
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                return new MacClipboardListener();
            }
#endif
            throw new PlatformNotSupportedException("Clipboard listener not supported on this platform.");
        }

        /// <summary>
        /// Creates an IClipboardListener implementation appropriate for the type and current platform.
        /// </summary>
        /// <typeparam name="T">IClipboardListener interface type for the platform.</typeparam>
        /// <returns>Implementation of IWindowsClipboardListener, ILinuxClipboardListener, or IMacClipboardListener depending on type and platform.</returns>
        /// <exception cref="NotSupportedException">Unsupported type for the platform or unsupported platform.</exception>
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
