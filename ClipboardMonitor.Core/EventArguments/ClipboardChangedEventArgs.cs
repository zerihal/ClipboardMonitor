﻿using ClipboardMonitor.Core.Enums;

namespace ClipboardMonitor.Core.EventArguments
{
    public class ClipboardChangedEventArgs : EventArgs
    {
        /// <summary>
        /// Clipboard text (if applicable).
        /// </summary>
        public string? ClipboardText { get; }

        /// <summary>
        /// Indicates whether the clipboard text is files or text.
        /// </summary>
        public bool IsClipboardTextFilesPath => DataType == ClipboardDataType.FILES;

        /// <summary>
        /// Indicates whether the notification type is notify clipboard changed only without data. If false, event args should contain data.
        /// </summary>
        public bool IsNotificationOnly => DataType == ClipboardDataType.NONE;

        /// <summary>
        /// Clipboard data type.
        /// </summary>
        public ClipboardDataType DataType { get; }

        /// <summary>
        /// Clipboard files (including path), if applicable.
        /// </summary>
        public IEnumerable<string>? ClipboardFiles
        {
            get
            {
                if (DataType == ClipboardDataType.FILES && !string.IsNullOrWhiteSpace(ClipboardText))
                    return ClipboardText.Split("\n");

                return null;
            }
        }

        public ClipboardChangedEventArgs(string clipboardText, ClipboardDataType type) : this(type)
        {
            ClipboardText = clipboardText;
        }

        public ClipboardChangedEventArgs(ClipboardDataType type)
        {
            DataType = type;
        }
    }
}
