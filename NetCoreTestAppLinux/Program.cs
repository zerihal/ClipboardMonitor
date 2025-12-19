using ClipboardMonitor.Core.Enums;
using ClipboardMonitor.Core.EventArguments;
using ClipboardMonitor.Core.Factories;
using ClipboardMonitor.Core.Interfaces;

namespace NetCoreTestAppLinux
{
    /// <summary>
    /// This is a temporary console app to test Linux clipboard listener implementation. Once verified, this will be combined
    /// with the original NetCoreTestApp project to have a single cross-platform test app.
    /// Basic testing done and OK - now need to tidy up and combine code!
    /// </summary>
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("*** Linux .NET Core Test App ***");
            IClipboardListener? clipboardListener = null;

            try
            {
                clipboardListener = ClipboardListenerFactory.CreateClipboardListener();

                // Verify that we have a Linux clipboard listener
                if (clipboardListener is ILinuxClipboardListener)
                {
                    Console.WriteLine("Linux Clipboard Listener created. Starting listener ...");
                    clipboardListener.ClipboardChanged += ClipboardListener_ClipboardChanged;
                    clipboardListener.Start();
                }
                else
                {
                    Console.WriteLine("Error: Created clipboard listener is not a Linux clipboard listener.");
                    clipboardListener = null;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            Console.WriteLine("Press Enter to exit...");
            Console.ReadLine();

            clipboardListener?.Stop();
        }

        private static void ClipboardListener_ClipboardChanged(object? sender, ClipboardChangedEventArgs e)
        {
            Console.WriteLine("Clipboard changed!");

            if (e.DataType == ClipboardDataType.IMAGE && e.ClipboardImage is IClipboardImage clipboardImage)
            {
                string folder = "/tmp/ClipboardTest";
                Directory.CreateDirectory(folder); // Ensure the folder exists
                string path = Path.Combine(folder, "clipboard_test.png");
                clipboardImage.Save(path);
                Console.WriteLine($"Image saved to: {path}");
            }
        }
    }
}
