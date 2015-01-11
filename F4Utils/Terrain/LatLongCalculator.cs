using System;

namespace F4Utils.Terrain
{
    public interface ILatLongCalculator
    {
        void CalculateLatLong(float baseLat, float baseLong, float feetNorth, float feetEast, out int latitudeWholeDegrees,
                                    out float latitudeFractionalMinutes, out int longitudeWholeDegrees,
                                    out float longitudeFactionalMinutes);
    }
    public class LatLongCalculator:ILatLongCalculator
    {
        public void CalculateLatLong(float baseLat, float baseLong, float feetNorth, float feetEast, out int latitudeWholeDegrees,
                                    out float latitudeFractionalMinutes, out int longitudeWholeDegrees,
                                    out float longitudeFactionalMinutes)
        {
           
            var theatreOriginLatitudeInDegrees = baseLat;
            var theatreOriginLongitudeInDegrees = baseLong;
            const float earthEquatorialRadiusInFeet = 2.09257E7F;
            const float feetPerMinuteOfLatLongAtEquator = 6087.03141F;
            const float feetPerDegreeOfLatLongAtEquator = feetPerMinuteOfLatLongAtEquator * 60.0F;
            const float radiansPerDegree = 0.01745329f;
            const float degreesPerRadian = 57.295780f;
            const float degreesPerMinute = 60.00f;

            var latitudeInRadians = (theatreOriginLatitudeInDegrees * feetPerDegreeOfLatLongAtEquator + feetNorth) /
                                    earthEquatorialRadiusInFeet;
            var cosineOfLatitude = (float)Math.Cos(latitudeInRadians);
            var longitudeInRadians = ((theatreOriginLongitudeInDegrees * radiansPerDegree * earthEquatorialRadiusInFeet *
                                       cosineOfLatitude) + feetEast) / (earthEquatorialRadiusInFeet * cosineOfLatitude);

            var latitudeInDegrees = latitudeInRadians * degreesPerRadian;
            var longitudeInDegrees = longitudeInRadians * degreesPerRadian;

            longitudeWholeDegrees = (int)Math.Floor(longitudeInDegrees);
            longitudeFactionalMinutes = Math.Abs(longitudeInDegrees - longitudeWholeDegrees) * degreesPerMinute;

            latitudeWholeDegrees = (int)Math.Floor(latitudeInDegrees);
            latitudeFractionalMinutes = Math.Abs(latitudeInDegrees - latitudeWholeDegrees) * degreesPerMinute;
        }

    }
}
