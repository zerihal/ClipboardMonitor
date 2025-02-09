using ClipboardMonitor.Core.Enums;
using System.Drawing;

namespace ClipboardMonitor.Core.EventArguments
{
    public class WinClipboardChangedEventArgs : ClipboardChangedEventArgs
    {
        /// <summary>
        /// Clipboard image (if applicable).
        /// </summary>
        public Bitmap? ClipboardImage { get; }

        public WinClipboardChangedEventArgs(Bitmap? clipboardImage, ClipboardDataType type) : base(type)
        {
            ClipboardImage = clipboardImage;
        }

        public WinClipboardChangedEventArgs(string clipboardText, ClipboardDataType type) : base(clipboardText, type) { }

        public WinClipboardChangedEventArgs(ClipboardDataType type) : base(type) { }
    }
}
