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
using F16CPD.Networking;
namespace F16CPD.SimSupport.Falcon4.MovingMap
{
    internal interface IMovingMap 
    {

        bool RenderMap(Graphics g, Rectangle renderRectangle, float mapScale,
            float mapCoordinateFeetEast, float mapCoordinateFeetNorth, float magneticHeadingInDecimalDegrees,
            int rangeRingDiameterInNauticalMiles, MapRotationMode rotationMode );
    }

    internal class MovingMap : IMovingMap
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof (MovingMap));
        private readonly IMapRingRenderer _mapRingRenderer;
        private readonly ICenterAirplaneRenderer _centerAirplaneRenderer;
        private readonly ITheaterMapRetriever _theaterMapRetriever;
        private readonly IF16CPDClient _client;
        private Bitmap _theaterMap;
        private float _mapWidthInFeet;
        public MovingMap( 
            TerrainDB terrainDB, 
            IF16CPDClient client,
            ITheaterMapRetriever theaterMapRetriever=null,
            IMapRingRenderer mapRingRenderer=null,
            ICenterAirplaneRenderer centerAirplaneRenderer=null
            )
        {
            _mapRingRenderer = mapRingRenderer ?? new MapRingRenderer();
            _centerAirplaneRenderer = centerAirplaneRenderer ?? new CenterAirplaneRenderer();
            _theaterMapRetriever = theaterMapRetriever ?? new TheaterMapRetriever(terrainDB, client);
        }

        public bool RenderMap(Graphics g, Rectangle renderRectangle, float mapScale, float mapCoordinateFeetEast, float mapCoordinateFeetNorth, float magneticHeadingInDecimalDegrees,
            int rangeRingDiameterInNauticalMiles, MapRotationMode rotationMode)
        {
            _theaterMap = _theaterMap ?? _theaterMapRetriever.GetTheaterMapImage(ref _mapWidthInFeet);
            if (_theaterMap == null) return false;
            var mapImageFeetPerPixel = _mapWidthInFeet / (float)_theaterMap.Width;
            var zoom = 1/(mapScale / 50000.0f) ;
            var scaleX = (float)renderRectangle.Width / ((float)_theaterMap.Width);
            var scaleY = (float)renderRectangle.Height / ((float)_theaterMap.Height);
            var xOffset = (-(mapCoordinateFeetEast / mapImageFeetPerPixel) + (((float)_theaterMap.Width / 2.0f)));
            var yOffset = (((mapCoordinateFeetNorth / mapImageFeetPerPixel) - (((float)_theaterMap.Height / 2.0f))));
            try
            {
                using (var renderTarget = new Bitmap(renderRectangle.Width, renderRectangle.Height, PixelFormat.Format16bppRgb565))
                {
                    using (var h = Graphics.FromImage(renderTarget))
                    {
                        var backgroundColor = Color.FromArgb(181,186,222);
                        h.PixelOffsetMode = PixelOffsetMode.Half;
                        h.Clear(backgroundColor);
                        h.TranslateTransform(renderTarget.Width / 2.0f, renderTarget.Height / 2.0f);
                        h.ScaleTransform(zoom, zoom);
                        h.TranslateTransform(-renderTarget.Width / 2.0f, -renderTarget.Height / 2.0f);
                        if (rotationMode == MapRotationMode.HeadingUp)
                        {
                            h.TranslateTransform(renderTarget.Width / 2.0f, renderTarget.Height / 2.0f);
                            h.RotateTransform(-(magneticHeadingInDecimalDegrees));
                            h.TranslateTransform(-renderTarget.Width / 2.0f, -renderTarget.Height / 2.0f);
                        }
                        
                       h.ScaleTransform(scaleX, scaleY);
                       h.TranslateTransform(xOffset, yOffset);
                       h.DrawImage(_theaterMap, 0, 0, new Rectangle(0, 0, _theaterMap.Width,_theaterMap.Height), GraphicsUnit.Pixel);
                    }
                    
                    g.DrawImageFast(
                        renderTarget,
                        renderRectangle,
                        new Rectangle(0, 0, renderTarget.Width, renderTarget.Height),
                        GraphicsUnit.Pixel
                        );

                    var originalTransform = g.Transform;
                    if (rotationMode == MapRotationMode.NorthUp)
                    {
                        g.TranslateTransform(renderRectangle.Width / 2.0f, renderRectangle.Height / 2.0f);
                        g.RotateTransform(-(magneticHeadingInDecimalDegrees));
                        g.TranslateTransform(-renderRectangle.Width / 2.0f, -renderRectangle.Height / 2.0f);
                    }
                    _centerAirplaneRenderer.DrawCenterAirplaneSymbol(g, renderRectangle);
                    g.Transform = originalTransform;

                    if (rotationMode == MapRotationMode.NorthUp)
                    {
                        g.TranslateTransform(renderRectangle.Width / 2.0f, renderRectangle.Height / 2.0f);
                        g.RotateTransform(magneticHeadingInDecimalDegrees);
                        g.TranslateTransform(-renderRectangle.Width / 2.0f, -renderRectangle.Height / 2.0f);
                    }
                    _mapRingRenderer.DrawMapRing(g, renderRectangle, 200, 1, magneticHeadingInDecimalDegrees);
                    g.Transform = originalTransform;
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message, ex);
                return false;
            }
            return true;
        }

    }
}