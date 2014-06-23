using F4Utils.Terrain.Structs;
using log4net;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace F4Utils.Terrain
{
    internal interface IFarTileBackgroundLoader : IDisposable
    {
        void LoadFarTilesAsync();
    }
    class FarTileBackgroundLoader:IFarTileBackgroundLoader
    {
        private bool _isDisposed=false;
        private static readonly ILog Log = LogManager.GetLogger(typeof(FarTileBackgroundLoader));
        private BackgroundWorker _backgroundWorker;
        private TerrainDB _terrainDB;
        private IDetailTextureForElevationPostRetriever _detailTextureForElevationPostRetriever;
        public FarTileBackgroundLoader(TerrainDB terrainDB, IDetailTextureForElevationPostRetriever detailTextureForElevationPostRetriever =null)
        {
            _terrainDB = terrainDB;
            _detailTextureForElevationPostRetriever = detailTextureForElevationPostRetriever ?? new DetailTextureForElevationPostRetriever();
        }
        public void LoadFarTilesAsync()
        {
            if (_backgroundWorker == null)
            {
                _backgroundWorker = new BackgroundWorker { WorkerSupportsCancellation = true };
                _backgroundWorker.DoWork += BackgroundWork;
            }
            if (_backgroundWorker.IsBusy) return;

            _backgroundWorker.RunWorkerAsync();
        }

        private void BackgroundWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                for (var lod = (_terrainDB.TheaterDotMap.LastNearTiledLOD + 1);
                     lod <= _terrainDB.TheaterDotMap.LastFarTiledLOD;
                     lod++)
                {
                    for (var x = 0;
                         x <
                         (_terrainDB.TheaterDotMap.LODMapWidths[lod] *
                          Constants.NUM_ELEVATION_POSTS_ACROSS_SINGLE_LOD_SEGMENT);
                         x++)
                    {
                        for (var y = 0;
                             y <
                             (_terrainDB.TheaterDotMap.LODMapHeights[lod] *
                              Constants.NUM_ELEVATION_POSTS_ACROSS_SINGLE_LOD_SEGMENT);
                             y++)
                        {
                            if (_backgroundWorker == null || _backgroundWorker.CancellationPending) return;
                            _detailTextureForElevationPostRetriever.GetDetailTextureForElevationPost(x, y, lod, _terrainDB);
                            if (y % 1024 == 0) Thread.Sleep(5);
                        }
                        Thread.Sleep(5);
                    }
                    Thread.Sleep(10);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message, ex);
                if (ex is SystemException) throw;
            }
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~FarTileBackgroundLoader()
        {
            Dispose();
        }

        private void Dispose(bool disposing)
        {
            if (!_isDisposed)
            {
                if (disposing)
                {
                    if (_backgroundWorker != null)
                    {
                        _backgroundWorker.CancelAsync();
                    }
                    var waitCount = 0;
                    while (_backgroundWorker != null && _backgroundWorker.IsBusy &&
                           waitCount < 1000)
                    {
                        Application.DoEvents();
                        Thread.Sleep(5);
                        waitCount++;
                    }
                    _backgroundWorker = null;
                }
            }
            // Code to dispose the un-managed resources of the class
            _isDisposed = true;
        }
    }
}
