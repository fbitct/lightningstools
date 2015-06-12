using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;
using Common.Imaging;
using F4Utils.Terrain;
using log4net;
using System.Drawing.Drawing2D;
using F4Utils.Resources;
using System.IO;
using F16CPD.FlightInstruments.Pfd;
namespace F16CPD.SimSupport.Falcon4.MovingMap
{
    internal interface IMovingMap : IDisposable
    {

        bool RenderMap(Graphics g, Rectangle renderRectangle, float mapScale,
            float mapCoordinateFeetEast, float mapCoordinateFeetNorth, float magneticHeadingInDecimalDegrees,
            int rangeRingDiameterInNauticalMiles, MapRotationMode rotationMode );
    }

    internal class MovingMap : IMovingMap
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof (MovingMap));
        private bool _isDisposed;
        private TerrainDB _terrainDB;
        private readonly IMapRingRenderer _mapRingRenderer;
        private readonly ICenterAirplaneRenderer _centerAirplaneRenderer;
        private readonly IResourceBundleReader _resourceBundleReader;
        public MovingMap( 
            TerrainDB terrainDB, 
            IMapRingRenderer mapRingRenderer=null,
            ICenterAirplaneRenderer centerAirplaneRenderer=null,
            IResourceBundleReader resourceBundleReader=null
            )
        {
            _terrainDB = terrainDB;
            _mapRingRenderer = mapRingRenderer ?? new MapRingRenderer();
            _centerAirplaneRenderer = centerAirplaneRenderer ?? new CenterAirplaneRenderer();
            _resourceBundleReader = resourceBundleReader ?? new ResourceBundleReader();

        }

        public bool RenderMap(Graphics g, Rectangle renderRectangle, float mapScale, float mapCoordinateFeetEast, float mapCoordinateFeetNorth, float magneticHeadingInDecimalDegrees,
            int rangeRingDiameterInNauticalMiles, MapRotationMode rotationMode)
        {
            if (_terrainDB == null || _terrainDB.TerrainBasePath == null) return false;
            var mapWidthInL2Segments = _terrainDB.TheaterDotMap.LODMapWidths[2];
            var mapWidthInL2Posts = mapWidthInL2Segments * F4Utils.Terrain.Constants.NUM_ELEVATION_POSTS_ACROSS_SINGLE_LOD_SEGMENT;
            var mapWidthInFeet = mapWidthInL2Posts * (_terrainDB.TheaterDotMap.FeetBetweenL0Posts*4);
            var mapImage = _terrainDB.GetTheaterMapImage(2);
            if (_resourceBundleReader.NumResources <= 0)
            {
                try
                {
                    var campMapResourceBundleIndexPath =
                                                         _terrainDB.DataPath + Path.DirectorySeparatorChar
                                                        + _terrainDB.TheaterDotTdf.artDir + Path.DirectorySeparatorChar
                                                        + "resource" + Path.DirectorySeparatorChar
                                                        + "campmap.idx";
                    _resourceBundleReader.Load(campMapResourceBundleIndexPath);
                }
                catch { }
            }
            if (_resourceBundleReader.NumResources > 0)
            {
                mapImage = _resourceBundleReader.GetImageResource("BIG_MAP_ID");
            }
            var mapImageFeetPerPixel = mapWidthInFeet / (float)mapImage.Width;
            var scaleX = (float)renderRectangle.Width / (float)mapImage.Width;
            var scaleY = (float)renderRectangle.Height / (float)mapImage.Height;
            var xOffset = -(mapCoordinateFeetEast / mapImageFeetPerPixel) + ((float)mapImage.Width / 2.0f);
            var yOffset = ((mapCoordinateFeetNorth / mapImageFeetPerPixel) - ((float)mapImage.Height / 2.0f));

            try
            {
                using (var renderTarget = new Bitmap(renderRectangle.Width, renderRectangle.Height, PixelFormat.Format16bppRgb565))
                {
                    using (var h = Graphics.FromImage(renderTarget))
                    {
                        var backgroundColor = Color.FromArgb(181,186,222);
                        h.PixelOffsetMode = PixelOffsetMode.Half;
                        h.Clear(backgroundColor);
                        if (rotationMode == MapRotationMode.CurrentHeadingOnTop)
                        {
                            h.TranslateTransform(renderTarget.Width / 2.0f, renderTarget.Height / 2.0f);
                            h.RotateTransform(-(magneticHeadingInDecimalDegrees));
                            h.TranslateTransform(-renderTarget.Width / 2.0f, -renderTarget.Height / 2.0f);
                        }
                        h.ScaleTransform(scaleX, scaleY);
                        h.DrawImageFast(mapImage, new PointF(xOffset, yOffset));
                        mapImage.Dispose();
                    }
                    g.DrawImageFast(
                        renderTarget,
                        renderRectangle,
                        new Rectangle(0,0,renderTarget.Width, renderTarget.Height),
                        GraphicsUnit.Pixel
                        );
                    _centerAirplaneRenderer.DrawCenterAirplaneSymbol(g, renderRectangle);

                    _mapRingRenderer.DrawMapRing(g, renderRectangle, 200, 1, magneticHeadingInDecimalDegrees);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message, ex);
                return false;
            }
            return true;
        }


        #region Destructors

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~MovingMap()
        {
            Dispose();
        }

        private void Dispose(bool disposing)
        {
            if (!_isDisposed)
            {
                if (disposing)
                {
                    //dispose of managed resources here
                    Common.Util.DisposeObject(_terrainDB);
                    _terrainDB = null;
                }
            }
            // Code to dispose the un-managed resources of the class
            _isDisposed = true;
        }

        #endregion


    }
}