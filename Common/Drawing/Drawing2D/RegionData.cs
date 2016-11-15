namespace Common.Drawing.Drawing2D
{
    /// <summary>Encapsulates the data that makes up a <see cref="T:Common.Drawing.Region" /> object. This class cannot be inherited.</summary>
    public sealed class RegionData
    {
        private System.Drawing.Drawing2D.RegionData WrappedRegionData { get; set; }
        private RegionData (System.Drawing.Drawing2D.RegionData regionData)
        {
            WrappedRegionData = regionData;
        }
        /// <summary>Gets or sets an array of bytes that specify the <see cref="T:Common.Drawing.Region" /> object.</summary>
        /// <returns>An array of bytes that specify the <see cref="T:Common.Drawing.Region" /> object.</returns>
        public byte[] Data
        {
            get { return WrappedRegionData.Data; }
            set { WrappedRegionData.Data = value; }
        }

        /// <summary>Converts the specified <see cref="T:System.Drawing.Drawing2D.RegionData" /> to a <see cref="T:Common.Drawing.Drawing2D.RegionData" />.</summary>
        /// <returns>The <see cref="T:Common.Drawing.Drawing2D.RegionData" /> that results from the conversion.</returns>
        /// <param name="regionData">The <see cref="T:System.Drawing.Drawing2D.RegionData" /> to be converted.</param>
        /// <filterpriority>3</filterpriority>
        public static implicit operator RegionData(System.Drawing.Drawing2D.RegionData regionData)
        {
            return new RegionData(regionData);
        }
        /// <summary>Converts the specified <see cref="T:Common.Drawing.Drawing2D.RegionData" /> to a <see cref="T:System.Drawing.Drawing2D.RegionData" />.</summary>
        /// <returns>The <see cref="T:System.Drawing.Drawing2D.RegionData" /> that results from the conversion.</returns>
        /// <param name="regionData">The <see cref="T:Common.Drawing.Drawing2D.RegionData" /> to be converted.</param>
        /// <filterpriority>3</filterpriority>
        public static implicit operator System.Drawing.Drawing2D.RegionData(RegionData regionData)
        {
            return regionData.WrappedRegionData;
        }
    }
}
