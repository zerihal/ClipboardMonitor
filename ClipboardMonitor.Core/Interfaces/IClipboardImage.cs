namespace ClipboardMonitor.Core.Interfaces
{
    public interface IClipboardImage
    {
        /// <summary>
        /// Image data in byte array format.
        /// </summary>
        byte[] Data { get; }

        /// <summary>
        /// Image format (e.g. "png", "jpeg").
        /// </summary>
        string Format { get; }

        /// <summary>
        /// Saves the current data to the specified file path.
        /// </summary>
        /// <param name="path">The file system path where the data will be saved. Cannot be null or empty.</param>
        /// <remarks>
        /// File extension given in the path will be checked against the image format. If this does not match, the file 
        /// extension will be changed to match the image format.
        /// </remarks>
        void Save(string path);
    }
}
