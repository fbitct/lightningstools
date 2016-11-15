using System;
using System.ComponentModel;

namespace Common.Drawing.Imaging
{
    /// <summary>Specifies the file format of the image. Not inheritable.</summary>
    public sealed class ImageFormat
    {
        private System.Drawing.Imaging.ImageFormat WrappedImageFormat { get; set; }
        private ImageFormat(System.Drawing.Imaging.ImageFormat imageFormat)
        {
            WrappedImageFormat = imageFormat;
        }
        /// <summary>Gets a <see cref="T:System.Guid" /> structure that represents this <see cref="T:Common.Drawing.Imaging.ImageFormat" /> object.</summary>
        /// <returns>A <see cref="T:System.Guid" /> structure that represents this <see cref="T:Common.Drawing.Imaging.ImageFormat" /> object.</returns>
        public Guid Guid { get { return WrappedImageFormat.Guid; } }

        /// <summary>Gets the format of a bitmap in memory.</summary>
        /// <returns>An <see cref="T:Common.Drawing.Imaging.ImageFormat" /> object that indicates the format of a bitmap in memory.</returns>
        public static ImageFormat MemoryBmp { get { return System.Drawing.Imaging.ImageFormat.MemoryBmp; } }

        /// <summary>Gets the bitmap (BMP) image format.</summary>
        /// <returns>An <see cref="T:Common.Drawing.Imaging.ImageFormat" /> object that indicates the bitmap image format.</returns>
        public static ImageFormat Bmp { get { return System.Drawing.Imaging.ImageFormat.Bmp; } }

        /// <summary>Gets the enhanced metafile (EMF) image format.</summary>
        /// <returns>An <see cref="T:Common.Drawing.Imaging.ImageFormat" /> object that indicates the enhanced metafile image format.</returns>
        public static ImageFormat Emf { get { return System.Drawing.Imaging.ImageFormat.Emf; } }

        /// <summary>Gets the Windows metafile (WMF) image format.</summary>
        /// <returns>An <see cref="T:Common.Drawing.Imaging.ImageFormat" /> object that indicates the Windows metafile image format.</returns>
        public static ImageFormat Wmf { get { return System.Drawing.Imaging.ImageFormat.Wmf; } }

        /// <summary>Gets the Graphics Interchange Format (GIF) image format.</summary>
        /// <returns>An <see cref="T:Common.Drawing.Imaging.ImageFormat" /> object that indicates the GIF image format.</returns>
        public static ImageFormat Gif { get { return System.Drawing.Imaging.ImageFormat.Gif; } }

        /// <summary>Gets the Joint Photographic Experts Group (JPEG) image format.</summary>
        /// <returns>An <see cref="T:Common.Drawing.Imaging.ImageFormat" /> object that indicates the JPEG image format.</returns>
        public static ImageFormat Jpeg { get { return System.Drawing.Imaging.ImageFormat.Jpeg; } }

        /// <summary>Gets the W3C Portable Network Graphics (PNG) image format.</summary>
        /// <returns>An <see cref="T:Common.Drawing.Imaging.ImageFormat" /> object that indicates the PNG image format.</returns>
        public static ImageFormat Png { get { return System.Drawing.Imaging.ImageFormat.Png; } }

        /// <summary>Gets the Tagged Image File Format (TIFF) image format.</summary>
        /// <returns>An <see cref="T:Common.Drawing.Imaging.ImageFormat" /> object that indicates the TIFF image format.</returns>
        public static ImageFormat Tiff { get { return System.Drawing.Imaging.ImageFormat.Tiff; } }

        /// <summary>Gets the Exchangeable Image File (Exif) format.</summary>
        /// <returns>An <see cref="T:Common.Drawing.Imaging.ImageFormat" /> object that indicates the Exif format.</returns>
        public static ImageFormat Exif { get { return System.Drawing.Imaging.ImageFormat.Exif; } }
        
        /// <summary>Gets the Windows icon image format.</summary>
        /// <returns>An <see cref="T:Common.Drawing.Imaging.ImageFormat" /> object that indicates the Windows icon image format.</returns>
        public static ImageFormat Icon { get { return System.Drawing.Imaging.ImageFormat.Icon; } }

        /// <summary>Initializes a new instance of the <see cref="T:Common.Drawing.Imaging.ImageFormat" /> class by using the specified <see cref="T:System.Guid" /> structure.</summary>
        /// <param name="guid">The <see cref="T:System.Guid" /> structure that specifies a particular image format. </param>
        public ImageFormat(Guid guid)
        {
            WrappedImageFormat = new System.Drawing.Imaging.ImageFormat(guid);
        }

        /// <summary>Returns a value that indicates whether the specified object is an <see cref="T:Common.Drawing.Imaging.ImageFormat" /> object that is equivalent to this <see cref="T:Common.Drawing.Imaging.ImageFormat" /> object.</summary>
        /// <returns>true if <paramref name="o" /> is an <see cref="T:Common.Drawing.Imaging.ImageFormat" /> object that is equivalent to this <see cref="T:Common.Drawing.Imaging.ImageFormat" /> object; otherwise, false.</returns>
        /// <param name="o">The object to test. </param>
        public override bool Equals(object o)
        {
            return WrappedImageFormat.Equals(o);
        }

        /// <summary>Returns a hash code value that represents this object.</summary>
        /// <returns>A hash code that represents this object.</returns>
        public override int GetHashCode()
        {
            return WrappedImageFormat.GetHashCode();
        }


        /// <summary>Converts this <see cref="T:Common.Drawing.Imaging.ImageFormat" /> object to a human-readable string.</summary>
        /// <returns>A string that represents this <see cref="T:Common.Drawing.Imaging.ImageFormat" /> object.</returns>
        public override string ToString()
        {
            return WrappedImageFormat.ToString();
        }

        /// <summary>Converts the specified <see cref="T:System.Drawing.Imaging.ImageFormat" /> to a <see cref="T:Common.Drawing.Imaging.ImageFormat" />.</summary>
        /// <returns>The <see cref="T:Common.Drawing.Imaging.ImageFormat" /> that results from the conversion.</returns>
        /// <param name="imageFormat">The <see cref="T:System.Drawing.Imaging.ImageFormat" /> to be converted.</param>
        /// <filterpriority>3</filterpriority>
        public static implicit operator ImageFormat(System.Drawing.Imaging.ImageFormat imageFormat)
        {
            return new ImageFormat(imageFormat);
        }

        /// <summary>Converts the specified <see cref="T:Common.Drawing.Imaging.ImageFormat" /> to a <see cref="T:System.Drawing.Imaging.ImageFormat" />.</summary>
        /// <returns>The <see cref="T:System.Drawing.Imaging.ImageFormat" /> that results from the conversion.</returns>
        /// <param name="imageFormat">The <see cref="T:Common.Drawing.Imaging.ImageFormat" /> to be converted.</param>
        /// <filterpriority>3</filterpriority>
        public static implicit operator System.Drawing.Imaging.ImageFormat(ImageFormat imageFormat)
        {
            return imageFormat.WrappedImageFormat;
        }

    }
}
