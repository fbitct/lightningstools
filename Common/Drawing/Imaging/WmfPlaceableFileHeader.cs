﻿using System;
using System.Runtime.InteropServices;

namespace Common.Drawing.Imaging
{
    /// <summary>Defines a placeable metafile. Not inheritable.</summary>
    [StructLayout(LayoutKind.Sequential)]
    public sealed class WmfPlaceableFileHeader
    {
        private System.Drawing.Imaging.WmfPlaceableFileHeader WrappedWmfPlaceableFileHeader { get; set; }
        private WmfPlaceableFileHeader(System.Drawing.Imaging.WmfPlaceableFileHeader wmfPlaceableFileHeader)
        {
            WrappedWmfPlaceableFileHeader = wmfPlaceableFileHeader;
        }
        /// <summary>Gets or sets a value indicating the presence of a placeable metafile header.</summary>
        /// <returns>A value indicating presence of a placeable metafile header.</returns>
        public int Key
        {
            get { return WrappedWmfPlaceableFileHeader.Key; }
            set { WrappedWmfPlaceableFileHeader.Key = value ; }
        }

        /// <summary>Gets or sets the handle of the metafile in memory.</summary>
        /// <returns>The handle of the metafile in memory.</returns>
        public short Hmf
        {
            get { return WrappedWmfPlaceableFileHeader.Hmf; }
            set { WrappedWmfPlaceableFileHeader.Hmf = value; }
        }

        /// <summary>Gets or sets the x-coordinate of the upper-left corner of the bounding rectangle of the metafile image on the output device.</summary>
        /// <returns>The x-coordinate of the upper-left corner of the bounding rectangle of the metafile image on the output device.</returns>
        public short BboxLeft
        {
            get { return WrappedWmfPlaceableFileHeader.BboxLeft; }
            set { WrappedWmfPlaceableFileHeader.BboxLeft = value; }
        }

        /// <summary>Gets or sets the y-coordinate of the upper-left corner of the bounding rectangle of the metafile image on the output device.</summary>
        /// <returns>The y-coordinate of the upper-left corner of the bounding rectangle of the metafile image on the output device.</returns>
        public short BboxTop
        {
            get { return WrappedWmfPlaceableFileHeader.BboxTop; }
            set { WrappedWmfPlaceableFileHeader.BboxTop = value; }
        }

        /// <summary>Gets or sets the x-coordinate of the lower-right corner of the bounding rectangle of the metafile image on the output device.</summary>
        /// <returns>The x-coordinate of the lower-right corner of the bounding rectangle of the metafile image on the output device.</returns>
        public short BboxRight
        {
            get { return WrappedWmfPlaceableFileHeader.BboxRight; }
            set { WrappedWmfPlaceableFileHeader.BboxRight = value; }
        }

        /// <summary>Gets or sets the y-coordinate of the lower-right corner of the bounding rectangle of the metafile image on the output device.</summary>
        /// <returns>The y-coordinate of the lower-right corner of the bounding rectangle of the metafile image on the output device.</returns>
        public short BboxBottom
        {
            get { return WrappedWmfPlaceableFileHeader.BboxBottom; }
            set { WrappedWmfPlaceableFileHeader.BboxBottom = value; }
        }

        /// <summary>Gets or sets the number of twips per inch.</summary>
        /// <returns>The number of twips per inch.</returns>
        public short Inch
        {
            get { return WrappedWmfPlaceableFileHeader.Inch; }
            set { WrappedWmfPlaceableFileHeader.Inch = value; }
        }

        /// <summary>Reserved. Do not use.</summary>
        /// <returns>Reserved. Do not use.</returns>
        public int Reserved
        {
            get { return WrappedWmfPlaceableFileHeader.Reserved; }
            set { WrappedWmfPlaceableFileHeader.Reserved = value; }
        }

        /// <summary>Gets or sets the checksum value for the previous ten WORD s in the header.</summary>
        /// <returns>The checksum value for the previous ten WORD s in the header.</returns>
        public short Checksum
        {
            get { return WrappedWmfPlaceableFileHeader.Checksum; }
            set { WrappedWmfPlaceableFileHeader.Checksum = value; }
        }

        /// <summary>Converts the specified <see cref="T:System.Drawing.Imaging.WmfPlaceableFileHeader" /> to a <see cref="T:Common.Drawing.Imaging.WmfPlaceableFileHeader" />.</summary>
        /// <returns>The <see cref="T:Common.Drawing.Imaging.WmfPlaceableFileHeader" /> that results from the conversion.</returns>
        /// <param name="wmfPlaceableFileHeader">The <see cref="T:System.Drawing.Imaging.WmfPlaceableFileHeader" /> to be converted.</param>
        /// <filterpriority>3</filterpriority>
        public static implicit operator WmfPlaceableFileHeader(System.Drawing.Imaging.WmfPlaceableFileHeader wmfPlaceableFileHeader)
        {
            return new WmfPlaceableFileHeader(wmfPlaceableFileHeader);
        }

        /// <summary>Converts the specified <see cref="T:Common.Drawing.Imaging.WmfPlaceableFileHeader" /> to a <see cref="T:System.Drawing.Imaging.WmfPlaceableFileHeader" />.</summary>
        /// <returns>The <see cref="T:System.Drawing.Imaging.WmfPlaceableFileHeader" /> that results from the conversion.</returns>
        /// <param name="wmfPlaceableFileHeader">The <see cref="T:Common.Drawing.Imaging.WmfPlaceableFileHeader" /> to be converted.</param>
        /// <filterpriority>3</filterpriority>
        public static implicit operator System.Drawing.Imaging.WmfPlaceableFileHeader(WmfPlaceableFileHeader wmfPlaceableFileHeader)
        {
            return wmfPlaceableFileHeader.WrappedWmfPlaceableFileHeader;
        }

    }
}
