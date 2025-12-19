using ClipboardMonitor.Core.EventArguments;

namespace ClipboardMonitor.Core.Interfaces
{
    public interface IWindowsClipboardListener : IClipboardListener
    {
        /// <summary>
        /// Flag to verify whether new image data is the same as previous image data that was in the clipboard
        /// (default <see langword="false"/>).
        /// </summary>
        /// <remarks>
        /// Note: If enabled, this will calculate a hash of any new image copied and compare it against the last
        /// calculated hash if previous clipboard content was also an image, however be aware that this will 
        /// slightly increase the processing time due to hash calculation, especially for large images. If the
        /// ommision of any additional image copy events is required, this flag can be enabled to handle this, 
        /// otherwise it is suggested that this is left as per default.
        /// </remarks>
        bool VerifyNewImageData { get; set; }
    }
}
