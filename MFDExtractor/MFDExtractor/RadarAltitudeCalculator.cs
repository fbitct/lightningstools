using F4SharedMem;
using F4SharedMem.Headers;
using F4Utils.Terrain;

namespace MFDExtractor
{
	internal interface IRadarAltitudeCalculator
	{
		float ComputeRadarAltitude(FlightData flightData);
	}

	class RadarAltitudeCalculator : IRadarAltitudeCalculator
	{
		private readonly ITerrainHeightProvider _terrainHeightProvider;
		public RadarAltitudeCalculator(ITerrainHeightProvider terrainHeightProvider = null)
		{
			_terrainHeightProvider = terrainHeightProvider ?? new TerrainBrowser(false);
		}
		public float ComputeRadarAltitude(FlightData flightData)
		{
			var terrainHeight = _terrainHeightProvider.GetTerrainHeight(flightData.x, flightData.y);
			var ralt = -flightData.z - terrainHeight;

			//reset AGL altitude to zero if we're on the ground
			if (
				WeightOnWheels(flightData) 
					||
				(OnGround(flightData) && flightData.DataFormat == FalconDataFormats.BMS4)
			)
			{
				ralt = 0;
			}

			if (ralt < 0)
			{
				ralt = 0;
			}
			return ralt;
		}

		private static bool OnGround(FlightData flightData)
		{
			return ((flightData.lightBits3 & (int)Bms4LightBits3.OnGround) ==(int)Bms4LightBits3.OnGround);
		}

		private static bool WeightOnWheels(FlightData flightData)
		{
			return ((flightData.lightBits & (int)LightBits.WOW) == (int)LightBits.WOW);
		}
	}
}
