using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Windows.Forms;
using F16CPD.Properties;
using F4Utils.Terrain;
using log4net;

namespace F16CPD.SimSupport.Falcon4
{
    internal interface IMovingMap : IDisposable
    {
        void RenderMap(Graphics g, Rectangle renderRectangle, float mapScale,
            int rangeRingDiameterInNauticalMiles, MapRotationMode rotationMode);

        bool RenderMapAsync(Graphics g, Rectangle renderRectangle, float mapScale,
            int rangeRingDiameterInNauticalMiles, MapRotationMode rotationMode, DoWorkEventArgs e);
    }

    internal class MovingMap : IMovingMap
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof (MovingMap));
        private readonly IDetailTextureForElevationPostRetriever _detailTextureForElevationPostRetriever;
        private readonly IElevationPostCoordinateClamper _elevationPostCoordinateClamper;
        private readonly object _mapImageLock = new object();
        private readonly F16CpdMfdManager _mfdManager;
        private readonly ITheaterMapBuilder _theaterMapBuilder;
        private bool _isDisposed;
        private float _lastMapScale;
        private Bitmap _lastRenderedMapImage;
        private Bitmap _mapAirplaneBitmap;
        private int _mapRenderProgress;
        private BackgroundWorker _mapRenderingBackgroundWorker;
        private DoWorkEventHandler _mapRenderingBackgroundWorkerDoWorkDelegate;
        private TerrainDB _terrainDB;

        public MovingMap(F16CpdMfdManager mfdManager, TerrainDB terrainDB, ITheaterMapBuilder theaterMapBuilder = null,
            IDetailTextureForElevationPostRetriever detailTextureForElevationPostRetriever = null,
            IElevationPostCoordinateClamper elevationPostCoordinateClamper = null)
        {
            _mfdManager = mfdManager;
            _terrainDB = terrainDB;
            _theaterMapBuilder = theaterMapBuilder ?? new TheaterMapBuilder();
            _detailTextureForElevationPostRetriever = detailTextureForElevationPostRetriever ??
                                                      new DetailTextureForElevationPostRetriever();
            _elevationPostCoordinateClamper = elevationPostCoordinateClamper ?? new ElevationPostCoordinateClamper();
        }

        public void RenderMap(Graphics g, Rectangle renderRectangle, float mapScale,
            int rangeRingDiameterInNauticalMiles, MapRotationMode rotationMode)
        {
            //set up a background worker to do map rendering, if we haven't already set one up
            if (_mapRenderingBackgroundWorker == null)
            {
                _mapRenderingBackgroundWorker = new BackgroundWorker();
                if (_mapRenderingBackgroundWorkerDoWorkDelegate == null)
                {
                    _mapRenderingBackgroundWorkerDoWorkDelegate =
                        MapRenderingBackgroundWorkerDoWork;
                }
                _mapRenderingBackgroundWorker.DoWork += _mapRenderingBackgroundWorkerDoWorkDelegate;
                _mapRenderingBackgroundWorker.WorkerSupportsCancellation = true;
                _mapRenderingBackgroundWorker.WorkerReportsProgress = true;
                _mapRenderingBackgroundWorker.ProgressChanged += MapRenderingBackgroundWorkerProgressChanged;
            }
            if (_lastMapScale != mapScale)
            {
                if (_mapRenderingBackgroundWorker.IsBusy)
                {
                    _mapRenderingBackgroundWorker.CancelAsync();
                }
            }
            _lastMapScale = mapScale;

            //TODO: break on changes in rotation mode, centering mode, etc.

            //if the background worker is not busy, have it go render another map for us
            if (!_mapRenderingBackgroundWorker.IsBusy)
            {
                var args = new MapRenderAsyncArguments
                {
                    RenderRectangle = renderRectangle,
                    MapScale = mapScale,
                    RangeRingDiameterInNauticalMiles = rangeRingDiameterInNauticalMiles,
                    RotationMode = rotationMode
                };
                _mapRenderingBackgroundWorker.RunWorkerAsync(args);
            }
            lock (_mapImageLock)
            {
                if (_lastRenderedMapImage != null)
                {
                    //in the meantime go render the last drawn map
                    g.DrawImage(_lastRenderedMapImage, renderRectangle,
                        new Rectangle(new Point(0, 0), _lastRenderedMapImage.Size), GraphicsUnit.Pixel);
                }
                else
                {
                    var gTransform = g.Transform;
                    g.ResetTransform();
                    var toDisplay = String.Format("LOADING: {0}%", _mapRenderProgress);
                    var greenBrush = Brushes.Green;
                    var path = new GraphicsPath();
                    var sf = new StringFormat(StringFormatFlags.NoWrap)
                    {
                        Alignment = StringAlignment.Center,
                        LineAlignment = StringAlignment.Center
                    };
                    var f = new Font(FontFamily.GenericMonospace, 20, FontStyle.Bold);
                    var textSize = g.MeasureString(toDisplay, f, 1, sf);
                    var leftX = (((renderRectangle.Width - ((int) textSize.Width))/2));
                    var topY = (((renderRectangle.Height - ((int) textSize.Height))/2));
                    var target = new Rectangle(leftX, topY, (int) textSize.Width, (int) textSize.Height);
                    path.AddString(toDisplay, f.FontFamily, (int) f.Style, f.Size, target.Location, sf);
                    g.FillPath(greenBrush, path);
                    g.Transform = gTransform;
                }
            }
        }

        public bool RenderMapAsync(Graphics g, Rectangle renderRectangle, float mapScale,
            int rangeRingDiameterInNauticalMiles, MapRotationMode rotationMode, DoWorkEventArgs e)
        {
            if (_terrainDB == null || _terrainDB.TerrainBasePath == null) return false;
            _mapRenderingBackgroundWorker.ReportProgress(0);

            //define original screen size in pixels and inches
            var originalRenderSizeInPixels = new Size(Constants.I_NATIVE_RES_WIDTH, Constants.I_NATIVE_RES_HEIGHT);
            var originalRenderSizeInScreenInches = new SizeF(6.24f, 8.32f);

            var originalRenderDiameterPixels =
                (float)
                    Math.Sqrt((originalRenderSizeInPixels.Width*originalRenderSizeInPixels.Width) +
                              (originalRenderSizeInPixels.Height*originalRenderSizeInPixels.Height));
            var originalRenderDiameterInScreenInches =
                (float)
                    Math.Sqrt((originalRenderSizeInScreenInches.Width*originalRenderSizeInScreenInches.Width) +
                              (originalRenderSizeInScreenInches.Height*originalRenderSizeInScreenInches.Height));

            //calculate how much terrain distance to render at specified scale in order to fill the screen 
            var terrainWidthToRenderInNauticalMiles = (originalRenderDiameterInScreenInches/(1.0000000000f/mapScale))/
                                                        Common.Math.Constants.INCHES_PER_NAUTICAL_MILE;
            var terrainWidthToRenderInFeet = terrainWidthToRenderInNauticalMiles*Common.Math.Constants.FEET_PER_NM;

            var outerMapRingDiameterPixelsUnscaled = ((rangeRingDiameterInNauticalMiles)/
                                                        terrainWidthToRenderInNauticalMiles)*
                                                       originalRenderDiameterPixels;


            //calculate number of elevation posts involved in rendering that amount of terrain distance
            var feetBetweenL0ElevationPosts = _terrainDB.TheaterDotMap.FeetBetweenL0Posts;
            var numL0ElevationPostsToRender =
                (int) (Math.Ceiling(terrainWidthToRenderInFeet/feetBetweenL0ElevationPosts));
            if (numL0ElevationPostsToRender < 1) return false;
            var posX = _mfdManager.FlightData.MapCoordinateFeetEast;
            var posY = _mfdManager.FlightData.MapCoordinateFeetNorth;

            //determine which Level of Detail to use for rendering the map
            //start with the lowest Level of Detail available (i.e. highest-resolution map) and work upward (i.e. toward lower-resolution maps covering greater areas)
            uint lod = 0;
            var feetBetweenElevationPosts = feetBetweenL0ElevationPosts;
            float numThisLodElevationPostsToRender = numL0ElevationPostsToRender;
            var thisLodDetailTextureWidthPixels = 64;

            const int numAdditionalElevationPostsToRender = 1;

            while ((numThisLodElevationPostsToRender*thisLodDetailTextureWidthPixels) > originalRenderDiameterPixels)
                //choose LoD that requires fewest unnecessary pixels to be rendered
            {
                if (lod + 1 > _terrainDB.TheaterDotMap.LastFarTiledLOD)
                {
                    break;
                }
                lod++;
                feetBetweenElevationPosts *= 2.0f;
                numThisLodElevationPostsToRender /= 2.0f;
                if (lod > _terrainDB.TheaterDotMap.LastNearTiledLOD)
                {
                    thisLodDetailTextureWidthPixels = 32;
                }
                else
                {
                    var sample = _detailTextureForElevationPostRetriever.GetDetailTextureForElevationPost(0, 0, lod,
                        _terrainDB);
                    thisLodDetailTextureWidthPixels = sample != null ? sample.Width : 1;
                }
            }

            //there's no such thing as a fractional elevation post, so round up to the next integral number of elevation posts
            numThisLodElevationPostsToRender = (float) Math.Ceiling(numThisLodElevationPostsToRender);
            numThisLodElevationPostsToRender += numAdditionalElevationPostsToRender;

            //determine which elevation post represents the map's center
            var centerXElevationPost = (int) Math.Floor(posX/feetBetweenElevationPosts);
            var centerYElevationPost = (int) Math.Floor(posY/feetBetweenElevationPosts);

            //now do the same thing but allow for the concept of fractional elevation posts
            var centerXElevationPostF = (posX/feetBetweenElevationPosts);
            var centerYElevationPostF = (posY/feetBetweenElevationPosts);

            //now calculate the difference between the fractional number and the integral number, to determine how far of an offset into a single elevation post's detail texture, we should take as
            //being the exact center of the map (we'll use these offsets to crop a sub-square of terrain out of a larger square of rendered terrain later in the process)
            var xOffset =
                (int) Math.Floor((((centerXElevationPostF - centerXElevationPost)*thisLodDetailTextureWidthPixels)));
            var yOffset =
                -(int) Math.Floor((((centerYElevationPostF - centerYElevationPost)*thisLodDetailTextureWidthPixels)));

            //determine the boundaries of the map section that we'll be rendering
            var leftXPost =
                (int)
                    Math.Floor(
                        (decimal) (centerXElevationPost - (int) Math.Floor(((numThisLodElevationPostsToRender/2.0f)))));
            var rightXPost =
                (int)
                    Math.Floor(
                        (decimal) (centerXElevationPost + (int) Math.Floor(((numThisLodElevationPostsToRender/2.0f)))));
            var topYPost =
                (int)
                    Math.Floor(
                        (decimal) (centerYElevationPost + (int) Math.Floor(((numThisLodElevationPostsToRender/2.0f)))));
            var bottomYPost =
                (int)
                    Math.Floor(
                        (decimal) (centerYElevationPost - (int) Math.Floor(((numThisLodElevationPostsToRender/2.0f)))));
            var clampLeftXPost = leftXPost;
            var clampRightXPost = rightXPost;
            var clampTopYPost = topYPost;
            var clampBottomYPost = bottomYPost;
            _elevationPostCoordinateClamper.ClampElevationPostCoordinates(ref clampLeftXPost, ref clampTopYPost, lod,
                _terrainDB);
            _elevationPostCoordinateClamper.ClampElevationPostCoordinates(ref clampRightXPost, ref clampBottomYPost, lod,
                _terrainDB);

            //now store those boundaries in a Size object for convenience
            var elevationPostsToRenderBoundsSize = new Size((Math.Abs(rightXPost - leftXPost)),
                (Math.Abs(bottomYPost - topYPost)));
            var clampedElevationPostsToRenderBoundsSize = new Size((Math.Abs(clampRightXPost - clampLeftXPost)),
                (Math.Abs(clampBottomYPost - clampTopYPost)));
            //determine the bounds of the cropped section of the render target that we'll be using 
            var cropWidth = ((elevationPostsToRenderBoundsSize.Width + 1 - numAdditionalElevationPostsToRender)*
                             thisLodDetailTextureWidthPixels);
            var toScale = 1.0f;
            if (cropWidth > originalRenderDiameterPixels)
            {
                var newCropWidth = (int) Math.Floor(originalRenderDiameterPixels);
                toScale = newCropWidth/(float) cropWidth;
                cropWidth = newCropWidth;
            }
            var scaleFactor = cropWidth/originalRenderDiameterPixels;

            //create the render target and render the overall (larger) map section to the render target
            try
            {
                using (var renderTarget = new Bitmap(cropWidth, cropWidth, PixelFormat.Format16bppRgb565))
                {
                    using (var h = Graphics.FromImage(renderTarget))
                    {
                        h.PixelOffsetMode = PixelOffsetMode.Half;
                        if (rotationMode == MapRotationMode.CurrentHeadingOnTop)
                        {
                            RotateDestinationGraphicsSurfaceToPutCurrentHeadingOnTop(cropWidth, h);
                        }

                        h.ScaleTransform(toScale, toScale);
                        h.TranslateTransform(-xOffset, -yOffset);

                        var crapMap =
                            _theaterMapBuilder.GetTheaterMap(_terrainDB.TheaterDotMap.NumLODs - 1, _terrainDB);
                        if (crapMap != null)
                        {
                            h.Clear(crapMap.GetPixel(crapMap.Width - 1, crapMap.Height - 1));
                        }
                        var numPostsRendered = 0;
                        for (var thisElevationPostY = clampBottomYPost;
                            thisElevationPostY <= clampTopYPost;
                            thisElevationPostY++)
                        {
                            for (var thisElevationPostX = clampLeftXPost;
                                thisElevationPostX <= clampRightXPost;
                                thisElevationPostX++)
                            {
                                if (_mapRenderingBackgroundWorker.CancellationPending)
                                {
                                    _mapRenderProgress = 0;
                                    e.Cancel = true;
                                    return false;
                                }

                                RenderMapTextureForCurrentElevationPost(lod, thisLodDetailTextureWidthPixels, leftXPost,
                                    topYPost, h, thisElevationPostY, thisElevationPostX);
                                numPostsRendered++;
                            }

                            _mapRenderingBackgroundWorker.ReportProgress(
                                (int) Math.Floor((
                                    numPostsRendered/
                                    (float) (
                                        clampedElevationPostsToRenderBoundsSize.Width
                                        *
                                        clampedElevationPostsToRenderBoundsSize.Height
                                        )
                                    )*100.0f)
                                );
                            Application.DoEvents();
                        }
                    }
                    var clipRectangle =
                        new Rectangle((cropWidth/2) - (int) ((originalRenderSizeInPixels.Width*scaleFactor)/2),
                            (cropWidth/2) - (int) ((originalRenderSizeInPixels.Height*scaleFactor)/2),
                            (int) (originalRenderSizeInPixels.Width*scaleFactor),
                            (int) (originalRenderSizeInPixels.Height*scaleFactor));
                    g.DrawImage(
                        renderTarget,
                        renderRectangle,
                        clipRectangle,
                        GraphicsUnit.Pixel
                        );
                    DrawAirplaneInCenter(g, renderRectangle);

                    var renderRectangleScaleFactor = (float) Math.Sqrt((renderRectangle.Width*renderRectangle.Width) +
                                                                         (renderRectangle.Height*renderRectangle.Height))/
                                                       originalRenderDiameterPixels;

                    DrawMapRing(g, renderRectangle, outerMapRingDiameterPixelsUnscaled, renderRectangleScaleFactor);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message, ex);
                return false;
            }
            return true;
        }

        private void MapRenderingBackgroundWorkerProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            _mapRenderProgress = e.ProgressPercentage;
        }

        private void MapRenderingBackgroundWorkerDoWork(object sender, DoWorkEventArgs e)
        {
            var args = (MapRenderAsyncArguments) e.Argument;
            var renderSurface = new Bitmap(args.RenderRectangle.Width, args.RenderRectangle.Height,
                PixelFormat.Format16bppRgb565);
            bool success;
            using (var g = Graphics.FromImage(renderSurface))
            {
                success = RenderMapAsync(g, args.RenderRectangle, args.MapScale, args.RangeRingDiameterInNauticalMiles,
                    args.RotationMode, e);
            }
            if (success)
            {
                lock (_mapImageLock)
                {
                    Common.Util.DisposeObject(_lastRenderedMapImage);
                    _lastRenderedMapImage = renderSurface;
                }
            }
            else
            {
                lock (_mapImageLock)
                {
                    Common.Util.DisposeObject(_lastRenderedMapImage);
                    _lastRenderedMapImage = null;
                }
            }
        }

        private void RotateDestinationGraphicsSurfaceToPutCurrentHeadingOnTop(int cropWidth, Graphics h)
        {
            h.TranslateTransform(cropWidth/2.0f, cropWidth/2.0f);
            h.RotateTransform(-(_mfdManager.FlightData.MagneticHeadingInDecimalDegrees));
            h.TranslateTransform(-cropWidth/2.0f, -cropWidth/2.0f);
        }

        private void RenderMapTextureForCurrentElevationPost(uint lod, int thisLodDetailTextureWidthPixels,
            int leftXPost, int topYPost, Graphics h, int thisElevationPostY, int thisElevationPostX)
        {
            //retrieve the detail texture corresponding to the current elevation post offset 
            var thisElevationPostDetailTexture =
                _detailTextureForElevationPostRetriever.GetDetailTextureForElevationPost
                    (thisElevationPostX, thisElevationPostY, lod, _terrainDB);
            if (thisElevationPostDetailTexture == null) return;
            //now draw the detail texture onto the render target
            var sourceRect = new Rectangle(0, 0, thisElevationPostDetailTexture.Width,
                thisElevationPostDetailTexture.Height);
            //determine the upper-left pixel at which to place this detail texture on the render target
            var destPoint = new Point(
                (thisElevationPostX - leftXPost)*thisLodDetailTextureWidthPixels,
                (topYPost - thisElevationPostY - 1)*thisLodDetailTextureWidthPixels
                );
            //calculate the destination rectangle (in pixels) on the render target, that we'll be placing the detail texture inside of
            var destRect = new Rectangle(destPoint,
                new Size(thisLodDetailTextureWidthPixels + 2,
                    thisLodDetailTextureWidthPixels + 2));
            h.DrawImage(thisElevationPostDetailTexture, destRect, sourceRect, GraphicsUnit.Pixel);
        }

        private void DrawMapRing(Graphics g, Rectangle renderRectangle, float outerMapRingDiameterPixelsUnscaled,
            float renderRectangleScaleFactor)
        {
            var mapRingPen = new Pen(Color.Magenta);
            var mapRingBrush = new SolidBrush(Color.Magenta);
            mapRingPen.Width = 1;
            const int mapRingLineWidths = 25;

            var originalGTransform = g.Transform;

            g.TranslateTransform(renderRectangle.Width/2.0f, renderRectangle.Height/2.0f);
            g.RotateTransform(-_mfdManager.FlightData.MagneticHeadingInDecimalDegrees);
            g.TranslateTransform(-renderRectangle.Width/2.0f, -renderRectangle.Height/2.0f);

            int outerMapRingDiameterPixelsScaled;
            DrawOuterMapRangeCircle(g, renderRectangle, outerMapRingDiameterPixelsUnscaled,
                out outerMapRingDiameterPixelsScaled, renderRectangleScaleFactor, mapRingPen, mapRingLineWidths);

            Rectangle innerMapRingBoundingRect;
            int innerMapRingBoundingRectMiddleX;
            DrawInnerMapRangeCircle(g, renderRectangle, mapRingPen, mapRingLineWidths,
                outerMapRingDiameterPixelsScaled, out innerMapRingBoundingRect, out innerMapRingBoundingRectMiddleX);
            DrawNorthMarkerOnInnerMapRangeCircle(g, mapRingBrush, innerMapRingBoundingRect,
                innerMapRingBoundingRectMiddleX);
            g.Transform = originalGTransform;
        }

        private static void DrawNorthMarkerOnInnerMapRangeCircle(Graphics g, Brush mapRingBrush,
            Rectangle innerMapRingBoundingRect, int innerMapRingBoundingRectMiddleX)
        {
            //draw north marker on inner map range circle
            var northMarkerPoints = new Point[3];
            northMarkerPoints[0] = new Point(innerMapRingBoundingRectMiddleX, innerMapRingBoundingRect.Top - 15);
            northMarkerPoints[1] = new Point(innerMapRingBoundingRectMiddleX - 12,
                innerMapRingBoundingRect.Top + 1);
            northMarkerPoints[2] = new Point(innerMapRingBoundingRectMiddleX + 12,
                innerMapRingBoundingRect.Top + 1);
            g.FillPolygon(mapRingBrush, northMarkerPoints);
        }

        private static void DrawInnerMapRangeCircle(Graphics g, Rectangle renderRectangle, Pen mapRingPen, int mapRingLineWidths, int outerMapRingDiameterPixelsScaled, out Rectangle innerMapRingBoundingRect, out int innerMapRingBoundingRectMiddleX)
        {
            //draw inner map range circle
            var innerMapRingDiameterPixelsScaled = (int) (Math.Floor(outerMapRingDiameterPixelsScaled/2.0f));
            innerMapRingBoundingRect =
                new Rectangle(((renderRectangle.Width - innerMapRingDiameterPixelsScaled)/2),
                    ((renderRectangle.Height - innerMapRingDiameterPixelsScaled)/2),
                    innerMapRingDiameterPixelsScaled, innerMapRingDiameterPixelsScaled);
            g.DrawEllipse(mapRingPen, innerMapRingBoundingRect);
            innerMapRingBoundingRectMiddleX = innerMapRingBoundingRect.X +
                                              (int) (Math.Floor(innerMapRingBoundingRect.Width/(float) 2));
            var innerMapRingBoundingRectMiddleY = innerMapRingBoundingRect.Y +
                                                  (int) (Math.Floor(innerMapRingBoundingRect.Height/(float) 2));
            g.DrawLine(mapRingPen, new Point(innerMapRingBoundingRect.X, innerMapRingBoundingRectMiddleY),
                new Point(innerMapRingBoundingRect.X + mapRingLineWidths, innerMapRingBoundingRectMiddleY));
            g.DrawLine(mapRingPen,
                new Point(innerMapRingBoundingRect.X + innerMapRingBoundingRect.Width,
                    innerMapRingBoundingRectMiddleY),
                new Point(
                    innerMapRingBoundingRect.X + innerMapRingBoundingRect.Width - mapRingLineWidths,
                    innerMapRingBoundingRectMiddleY));
            g.DrawLine(mapRingPen, new Point(innerMapRingBoundingRectMiddleX, innerMapRingBoundingRect.Bottom),
                new Point(innerMapRingBoundingRectMiddleX,
                    innerMapRingBoundingRect.Bottom - mapRingLineWidths));
            return;
        }

        private static void DrawOuterMapRangeCircle(Graphics g, Rectangle renderRectangle,
            float outerMapRingDiameterPixelsUnscaled, out int outerMapRingDiameterPixelsScaled,
            float renderRectangleScaleFactor, Pen mapRingPen, int mapRingLineWidths)
        {
            //rotate 45 degrees before drawing outer map range circle
            var preRotate = g.Transform;
            //capture current rotation so we can set it back before drawing inner map range circle
            g.TranslateTransform(renderRectangle.Width/2.0f, renderRectangle.Height/2.0f);
            g.RotateTransform(-45);
            g.TranslateTransform(-renderRectangle.Width/2.0f, -renderRectangle.Height/2.0f);

            //now draw outer map range circle
            outerMapRingDiameterPixelsScaled =
                (int) Math.Floor(outerMapRingDiameterPixelsUnscaled*renderRectangleScaleFactor);
            var outerMapRingBoundingRect =
                new Rectangle(((renderRectangle.Width - outerMapRingDiameterPixelsScaled)/2),
                    ((renderRectangle.Height - outerMapRingDiameterPixelsScaled)/2),
                    outerMapRingDiameterPixelsScaled, outerMapRingDiameterPixelsScaled);
            g.DrawEllipse(mapRingPen, outerMapRingBoundingRect);
            var outerMapRingBoundingRectMiddleX = outerMapRingBoundingRect.X +
                                                  (int) (Math.Floor(outerMapRingBoundingRect.Width/(float) 2));
            var outerMapRingBoundingRectMiddleY = outerMapRingBoundingRect.Y +
                                                  (int) (Math.Floor(outerMapRingBoundingRect.Height/(float) 2));
            g.DrawLine(mapRingPen, new Point(outerMapRingBoundingRectMiddleX, outerMapRingBoundingRect.Top),
                new Point(outerMapRingBoundingRectMiddleX,
                    outerMapRingBoundingRect.Top + mapRingLineWidths));
            g.DrawLine(mapRingPen, new Point(outerMapRingBoundingRect.X, outerMapRingBoundingRectMiddleY),
                new Point(outerMapRingBoundingRect.X + mapRingLineWidths, outerMapRingBoundingRectMiddleY));
            g.DrawLine(mapRingPen,
                new Point(outerMapRingBoundingRect.X + outerMapRingBoundingRect.Width,
                    outerMapRingBoundingRectMiddleY),
                new Point(
                    outerMapRingBoundingRect.X + outerMapRingBoundingRect.Width - mapRingLineWidths,
                    outerMapRingBoundingRectMiddleY));
            g.DrawLine(mapRingPen, new Point(outerMapRingBoundingRectMiddleX, outerMapRingBoundingRect.Bottom),
                new Point(outerMapRingBoundingRectMiddleX,
                    outerMapRingBoundingRect.Bottom - mapRingLineWidths));

            //set rotation back before drawing inner map range circle
            g.Transform = preRotate;
        }

        private void DrawAirplaneInCenter(Graphics g, Rectangle renderRectangle)
        {
            if (_mapAirplaneBitmap == null)
            {
                _mapAirplaneBitmap = (Bitmap) Resources.F16Symbol.Clone();
                _mapAirplaneBitmap.MakeTransparent(Color.FromArgb(255, 0, 255));
                _mapAirplaneBitmap =
                    (Bitmap)
                        Common.Imaging.Util.ResizeBitmap(_mapAirplaneBitmap,
                            new Size(
                                (int) Math.Floor(((float) _mapAirplaneBitmap.Width)),
                                (int) Math.Floor(((float) _mapAirplaneBitmap.Height))));
            }
            g.DrawImage(_mapAirplaneBitmap, (((renderRectangle.Width - _mapAirplaneBitmap.Width)/2)),
                (((renderRectangle.Height - _mapAirplaneBitmap.Height)/2)));
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
                    Common.Util.DisposeObject(_mapRenderingBackgroundWorkerDoWorkDelegate);
                    _mapRenderingBackgroundWorkerDoWorkDelegate = null;
                    Common.Util.DisposeObject(_mapRenderingBackgroundWorker);
                    _mapRenderingBackgroundWorker = null;
                }
            }
            // Code to dispose the un-managed resources of the class
            _isDisposed = true;
        }

        #endregion

        #region Nested type: MapRenderAsyncArguments

        private struct MapRenderAsyncArguments
        {
            public float MapScale;
            public int RangeRingDiameterInNauticalMiles;
            public Rectangle RenderRectangle;
            public MapRotationMode RotationMode;
        }

        #endregion
    }
}