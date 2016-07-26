using GeoCoordinatePortable;
using PokemonGoSlackService.Services.Interfaces;
using System;

namespace PokemonGoSlackService.Services
{
    public class BearingService : IBearingService
    {
        public string DegreeBearing(GeoCoordinate pointOne, GeoCoordinate pointTwo)
        {
            var dLon = ToRad(pointTwo.Longitude - pointOne.Longitude);
            var dPhi = Math.Log(Math.Tan(ToRad(pointTwo.Latitude) / 2 + Math.PI / 4) / Math.Tan(ToRad(pointOne.Latitude) / 2 + Math.PI / 4));

            if (Math.Abs(dLon) > Math.PI)
            {
                dLon = dLon > 0 ? -(2 * Math.PI - dLon) : (2 * Math.PI + dLon);

            }

            var bearing = ToBearing(Math.Atan2(dLon, dPhi));

            var directions = new string[] {
                "n", "ne", "e", "se", "s", "sw", "w", "nw", "n"
            };

            int DegreesPerDirection = 360 / (directions.Length - 1);
            var bearingIndex = (Convert.ToInt32(bearing) + (DegreesPerDirection / 2)) / DegreesPerDirection;

            return directions[bearingIndex];
        }

        private double ToBearing(double radians)
        {
            // convert radians to degrees (as bearing: 0...360)
            return (ToDegrees(radians) + 360) % 360;
        }

        private double ToDegrees(double radians)
        {
            return radians * 180 / Math.PI;
        }

        private double ToRad(double degrees)
        {
            return degrees * (Math.PI / 180);
        }
    }
}
