namespace ConstellationSimulator.Positions
{
    internal class Coordinate
    {
        public Coordinate(double latitude, double longitude, double altitude)
        {
            Latitude = latitude;
            Longitude = longitude;
            Altitude = altitude;
        }

        public double Latitude {
            get => _lat;
            set {
                if (value > 90 || value < -90)
                {
                    _lat = 0;
                }
                else
                {
                    _lat = value;
                }
            }
        }

        public double Longitude {
            get => _lon;
            set {
                if (value > 180 || value < -180)
                {
                    _lon = 0;
                }
                else
                {
                    _lon = value;
                }
            }
        }

        public double Altitude {
            get => _alt;
            set => _alt = (value > 0 ? value : 0);
        }

        private double _lat;
        private double _lon;
        private double _alt;
    }    
}
