using System;

namespace CircleScanner
{
    /// <summary>
    /// 经纬度上下限
    /// </summary>
    public class LatLonRange
    {
        /// <summary>
        /// 纬度下限
        /// </summary>
        public double LatMin;

        /// <summary>
        /// 纬度上限
        /// </summary>
        public double LatMax;

        /// <summary>
        /// 经度下限
        /// </summary>
        public double LonMin;

        /// <summary>
        /// 经度上限
        /// </summary>
        public double LonMax;
    }

    public class LatLonHelper
    {
        /// <summary>
        /// Radius of earth in m according to http://en.wikipedia.org/wiki/Earth_radius#Mean_radii
        /// </summary> 
        private static double R = 6371009.0;

        /// <summary>
        /// scalar to enlarge the rectangle defined by the start and end points
        /// </summary>
        private static double scalar = 1.2;

        /// <summary>
        /// minimal distance of the half of the length of the rectangle edges
        /// </summary>
        private static double minHalfLength = 500.0;

        public static double LatM2D(double m = 1.0)
        {
            return m * 180.0 / (3.1415926535897931 * LatLonHelper.R);
        }

        /// <summary>
        /// The assumpution is that the lat doesn't change a lot
        /// </summary>
        /// <param name="lat"></param>
        /// <param name="m"></param>
        /// <returns></returns>
        public static double LonM2D(double lat, double m = 1.0)
        {
            double latR = Math.Cos(lat * 3.1415926535897931 / 180.0) * LatLonHelper.R;
            return m * 180.0 / (3.1415926535897931 * latR);
        }

        /// <summary>
        /// find the distance between 2 point on map with lat-lon. lat-lon are given in degrees (360 based)
        /// </summary>
        /// <param name="lon1"></param>
        /// <param name="lat1"></param>
        /// <param name="lon2"></param>
        /// <param name="lat2"></param>
        /// <returns> distance in meters </returns>
        public static double FindDistance(double lon1, double lat1, double lon2, double lat2)
        {
            double midLat = (lat1 + lat2) / 2.0;
            double latDist = (lat1 - lat2) / LatLonHelper.LatM2D(1.0);
            double lonDist = (lon1 - lon2) / LatLonHelper.LonM2D(midLat, 1.0);
            return Math.Sqrt(latDist * latDist + lonDist * lonDist);
        }

        /// <summary>
        /// find the rectangle range defined 2 point on map with lat-lon. lat-lon are given in degrees (360 based)
        /// </summary>
        /// <param name="lon1"></param>
        /// <param name="lat1"></param>
        /// <param name="lon2"></param>
        /// <param name="lat2"></param>
        /// <returns></returns>
        public static LatLonRange FindLatLonRange(double lon1, double lat1, double lon2, double lat2)
        {
            double midLon = (lon1 + lon2) / 2.0;
            double midLat = (lat1 + lat2) / 2.0;
            double halfLonRange = Math.Max(LatLonHelper.scalar * Math.Abs(lon1 - midLon), Math.Abs(LatLonHelper.LonM2D(midLat, LatLonHelper.minHalfLength)));
            double halfLatRange = Math.Max(LatLonHelper.scalar * Math.Abs(lat1 - midLat), Math.Abs(LatLonHelper.LatM2D(LatLonHelper.minHalfLength)));
            return new LatLonRange
            {
                LatMin = midLat - halfLatRange,
                LatMax = midLat + halfLatRange,
                LonMin = midLon - halfLonRange,
                LonMax = midLon + halfLonRange
            };
        }
    }
}
