using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;
using F4Utils.Terrain;
using log4net;

namespace F16CPD.SimSupport.Falcon4.MovingMap
{
    internal interface IMovingMap : IDisposable
    {
        void RenderMap(Graphics g, Rectangle renderRectangle, float mapScale,
            float mapCoordinateFeetEast, float mapCoordinateFeetNorth, float magneticHeadingInDecimalDegrees,
            int rangeRingDiameterInNauticalMiles, MapRotationMode rotationMode);

        bool RenderMapAsync(Graphics g, Rectangle renderRectangle, float mapScale,
            float mapCoordinateFeetEast, float mapCoordinateFeetNorth, float magneticHeadingInDecimalDegrees,
            int rangeRingDiameterInNauticalMiles, MapRotationMode rotationMode, DoWorkEventArgs e );
    }

    internal class MovingMap : IMovingMap
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof (MovingMap));
        private readonly IDetailTextureForElevationPostRetriever _detailTextureForElevationPostRetriever;
        private readonly IElevationPostCoordinateClamper _elevationPostCoordinateClamper;
        private readonly object _mapImageLock = new object();
        private bool _isDisposed;
        private float _lastMapScale;
        private Bitmap _lastRenderedMapImage;
        private int _mapRenderProgress;
        private BackgroundWorker _mapRenderingBackgroundWorker;
        private DoWorkEventHandler _mapRenderingBackgroundWorkerDoWorkDelegate;
        private TerrainDB _terrainDB;
        private readonly IMapTextureRenderer _mapTextureRenderer;
        private readonly IMapLoadingMessageRenderer _mapLoadingMessageRenderer;
        private readonly IMapRingRenderer _mapRingRenderer;
        private readonly ICenterAirplaneRenderer _centerAirplaneRenderer;
        private readonly IMovingMapCanvasPreparer _movingMapCanvasPreparer;
        public MovingMap( 
            TerrainDB terrainDB, 
            IDetailTextureForElevationPostRetriever detailTextureForElevationPostRetriever = null,
            IElevationPostCoordinateClamper elevationPostCoordinateClamper = null,
            IMapTextureRenderer mapTextureRenderer=null,
            IMapLoadingMessageRenderer mapLoadingMessageRenderer = null,
            IMapRingRenderer mapRingRenderer=null,
            ICenterAirplaneRenderer centerAirplaneRenderer=null,
            IMovingMapCanvasPreparer movingMapCanvasPreparer=null)
        {
            _terrainDB = terrainDB;
            _elevationPostCoordinateClamper = elevationPostCoordinateClamper ?? new ElevationPostCoordinateClamper();
            _detailTextureForElevationPostRetriever = detailTextureForElevationPostRetriever ?? new DetailTextureForElevationPostRetriever();
            _mapTextureRenderer = mapTextureRenderer ?? new MapTextureRenderer(terrainDB, detailTextureForElevationPostRetriever);
            _mapLoadingMessageRenderer = mapLoadingMessageRenderer ?? new MapLoadingMessageRenderer();
            _mapRingRenderer = mapRingRenderer ?? new MapRingRenderer();
            _centerAirplaneRenderer = centerAirplaneRenderer ?? new CenterAirplaneRenderer();
            _movingMapCanvasPreparer = movingMapCanvasPreparer ?? new MovingMapCanvasPreparer(_terrainDB);
        }

        public void RenderMap(Graphics g, Rectangle renderRectangle, float mapScale, float mapCoordinateFeetNorth, float mapCoordinateFeetEast, float magneticHeadingDecimalDegrees,
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
                    RotationMode = rotationMode,
                    MapCoordinateFeetEast =  mapCoordinateFeetEast,
                    MapCoordinateFeetNorth = mapCoordinateFeetNorth,
                    MagneticHeadingDecimalDegrees = magneticHeadingDecimalDegrees
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
                    _mapLoadingMessageRenderer.RenderLoadingMessage(g, renderRectangle, _mapRenderProgress);
                }
            }
        }

        public bool RenderMapAsync(Graphics g, Rectangle renderRectangle, float mapScale, float mapCoordinateFeetEast, float mapCoordinateFeetNorth, float magneticHeadingInDecimalDegrees,
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
            var posX = mapCoordinateFeetEast;
            var posY = mapCoordinateFeetNorth;

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
                        _movingMapCanvasPreparer.PrepareCanvas(magneticHeadingInDecimalDegrees, rotationMode, h, cropWidth, toScale, xOffset, yOffset);
                        if (!RenderAllElevationPosts(
                            e, 
                            clampBottomYPost, clampTopYPost, clampLeftXPost, 
                            clampRightXPost, lod, thisLodDetailTextureWidthPixels, 
                            leftXPost, topYPost, h, clampedElevationPostsToRenderBoundsSize)) return false;
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
                    _centerAirplaneRenderer.DrawCenterAirplaneSymbol(g, renderRectangle);

                    var renderRectangleScaleFactor = (float) Math.Sqrt((renderRectangle.Width*renderRectangle.Width) +
                                                                         (renderRectangle.Height*renderRectangle.Height))/
                                                       originalRenderDiameterPixels;

                    _mapRingRenderer.DrawMapRing(g, renderRectangle, outerMapRingDiameterPixelsUnscaled, renderRectangleScaleFactor, magneticHeadingInDecimalDegrees);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message, ex);
                return false;
            }
            return true;
        }


        private bool RenderAllElevationPosts(DoWorkEventArgs e, int clampBottomYPost, int clampTopYPost, int clampLeftXPost,
            int clampRightXPost, uint lod, int thisLodDetailTextureWidthPixels, int leftXPost, int topYPost, Graphics h,
            Size clampedElevationPostsToRenderBoundsSize)
        {
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

                    _mapTextureRenderer.RenderMapTextureForCurrentElevationPost(lod, thisLodDetailTextureWidthPixels, leftXPost,
                        topYPost, h, thisElevationPostY, thisElevationPostX);
                    numPostsRendered++;
                }

                ReportProgress(numPostsRendered, clampedElevationPostsToRenderBoundsSize);
                Application.DoEvents();
            }
            return true;
        }

        private void ReportProgress(int numPostsRendered, Size clampedElevationPostsToRenderBoundsSize)
        {
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
                success = RenderMapAsync(g, args.RenderRectangle, args.MapScale, args.MapCoordinateFeetEast, args.MapCoordinateFeetNorth, args.MagneticHeadingDecimalDegrees,  args.RangeRingDiameterInNauticalMiles,
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
            public float MapCoordinateFeetNorth;
            public float MapCoordinateFeetEast;
            public float MagneticHeadingDecimalDegrees;
        }

        #endregion
    }
}