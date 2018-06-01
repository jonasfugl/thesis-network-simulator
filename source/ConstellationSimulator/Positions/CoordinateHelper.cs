using System;
using ConstellationSimulator.Configuration;

namespace ConstellationSimulator.Positions
{
    internal static class CoordinateHelper
    {
        /// <summary>
        /// Enum of all possible hemispheres in the simulator.
        /// </summary>
        public enum Hemisphere
        {
            NorthWest,
            NorthEast,
            SouthWest,
            SouthEast
        }

        /// <summary>
        /// Calculates the correct hemisphere, a given coordinate is located in.
        /// </summary>
        /// <param name="location">The location for which the hemisphere is calculated.</param>
        /// <returns></returns>
        public static Hemisphere CalcHemisphere(Coordinate location)
        {
            if(location.Latitude >= 0 && location.Longitude >= 0)
                return Hemisphere.NorthEast;
            if (location.Latitude >= 0 && location.Longitude < 0)
                return Hemisphere.NorthWest;
            if (location.Latitude < 0 && location.Longitude >= 0)
                return Hemisphere.SouthEast;
            if (location.Latitude < 0 && location.Longitude <= 0)
                return Hemisphere.SouthWest;

            throw new ArgumentOutOfRangeException(nameof(location));
        }

        /// <summary>
        /// Calculate propagation delay for a radio message between the two coordinates.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="destination"></param>
        /// <returns></returns>
        public static double CalculatePropagationDelayInMilliseconds(Coordinate source, Coordinate destination)
        {
            var distance = DistanceBetweenCoordinatesKilometer(source, destination); //km
            var speed = 299792.458; //km/s

            return (distance / speed) * 1000; //Returned in milliseconds
        }

        /// <summary>
        /// Calculates the distance between the two coordinates in kilometers.
        /// </summary>
        /// <param name="begin"></param>
        /// <param name="end"></param>
        /// <returns>Distance in kilometers</returns>
        private static double DistanceBetweenCoordinatesKilometer(Coordinate begin, Coordinate end)
        {
        //Method adapted from http://www.movable-type.co.uk/scripts/latlong.html
        var lat1 = ToRadian(begin.Latitude);
            var lat2 = ToRadian(end.Latitude);

            var deltaLatitude = DiffRadian(begin.Latitude, end.Latitude);
            var deltaLongitude = DiffRadian(begin.Longitude, end.Longitude);

            var a = Math.Sin(deltaLatitude / 2) * Math.Sin(deltaLatitude / 2) +
                    Math.Cos(lat1) * Math.Cos(lat2) *
                    Math.Sin(deltaLongitude / 2) * Math.Sin(deltaLongitude / 2);

            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

            return (SimulationConstants.EarthRadiusKilometer + begin.Altitude) * c;
        }

        /// <summary>
        /// Converts a value from degrees to radians.
        /// </summary>
        private static double ToRadian(double degree)
        {
            return degree * (Math.PI / 180);
        }

        /// <summary>
        /// Converts to degree values to radians, and returns the difference between them in radians.
        /// </summary>
        private static double DiffRadian(double val1, double val2)
        {
            return ToRadian(val2) - ToRadian(val1);
        }
    }
}
