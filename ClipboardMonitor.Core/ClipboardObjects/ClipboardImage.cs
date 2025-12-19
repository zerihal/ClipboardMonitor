using ClipboardMonitor.Core.Helpers;
using ClipboardMonitor.Core.Interfaces;

namespace ClipboardMonitor.Core.ClipboardObjects
{
    public class ClipboardImage : IClipboardImage
    {
        /// <inheritdoc/>
        public byte[] Data { get; }

        /// <inheritdoc/>
        public string Format { get; }

        /// <summary>
        /// Creates a new instance of ClipboardImage.
        /// </summary>
        /// <param name="data">Image data.</param>
        /// <param name="format">Image format (e.g. png, jpeg, etc).</param>
        public ClipboardImage(byte[] data, string format)
        {
            Data = data;
            Format = format;
        }

        /// <inheritdoc/>
        public virtual void Save(string path)
        {
            if (Path.GetExtension(path).ToLower() != $".{Format.ToLower()}")
                Path.ChangeExtension(path, Format);

            File.WriteAllBytes(path, Data);
        }

        /// <summary>
        /// Creates a ClipboardImage from raw clipboard data, also detecting the image format.
        /// </summary>
        /// <param name="data">Image data.</param>
        /// <param name="size">Image data size.</param>
        /// <returns>
        /// New instance of <see cref="IClipboardImage"/> from the data given, or null if the 
        /// image data could not be processed.
        /// </returns>
        public static ClipboardImage FromClipboardData(byte[] data, int size)
        {
            try
            {
                if (data == null || size <= 0 || size > data.Length)
                    throw new ArgumentException("Invalid data or size.");

                // Copy the relevant portion of the array
                byte[] imageData = new byte[size];
                Array.Copy(data, 0, imageData, 0, size);

                // Detect format from header (magic numbers)
                string format = ImageHelper.DetectImageFormat(imageData);

                return new ClipboardImage(imageData, format);
            }
            catch (Exception e)
            {
                Console.WriteLine("Error processing clipboard image data: " + e.Message); // Debug
            }

            return null!;
        }
    }
}
