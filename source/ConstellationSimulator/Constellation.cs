using System.Collections.Generic;
using ConstellationSimulator.Configuration;
using ConstellationSimulator.Positions;
using ConstellationSimulator.Statistics;

namespace ConstellationSimulator
{
    internal class Constellation
    {
        public Constellation(SimulationConfiguration conf, StatisticsCollector results)
        {
            //Create all locations for satellites
            //First orbit is at 0 longitude
            //Create list of all positions in order, and pass to satellite (longitude stays the same)

            _configuration = conf;
            _constellation = new List<List<Satellite>>(conf.NumberOfOrbits);

            for (var i = 0; i < conf.NumberOfOrbits; i++) //For each orbit (longitude, from 0-180 degrees)
            {
                var orbit = new List<Satellite>(conf.NumberOfSatellitesPerOrbit);

                //Create a list of latitudes (Argument of perigee in STK)
                for (var j = 0; j < conf.NumberOfSatellitesPerOrbit; j++) //Add a satellite to the orbit
                {
                    var perigee = j * conf.AngleBetweenSatellites;
                    var latitude = perigee;
                    var longitude = i * conf.AngleBetweenOrbits;

                    if (latitude > 270) //Change from of east/west hemisphere
                    {
                        latitude = -90 + (latitude - 270);
                    }
                    else if (latitude > 90) //Change from of east/west hemisphere
                    {
                        latitude = 90 - (latitude - 90);
                        longitude = -180 + (i * conf.AngleBetweenOrbits);
                    }

                    var coordinate = new Coordinate(latitude, longitude, conf.SatelliteAltitude);
                    var position = new SatPosition(i, j);
                    var name = "R" + i + "S" + j;

                    //Create satellite with new position
                    var sat = new Satellite(name, coordinate, position, position, ref _constellation, ref conf, results); //Initially, satposition and logical position are identical, however, only logical is incremented on each tick
                    orbit.Add(sat);
                }

                _constellation.Add(orbit);
            }
        }

        /// <summary>
        /// Runs one tick of the simulation.
        /// </summary>
        public void Tick()
        {
            foreach (var orbit in _constellation)
            {
                foreach (var sat in orbit)
                {
                    sat.Run();
                }
            }
        }

        /// <summary>
        /// Prints the routing tables of all satellites.
        /// </summary>
        /// <param name="path"></param>
        public void PrintRoutingTables(string path)
        {
            foreach (var orbit in _constellation)
            {
                foreach (var sat in orbit)
                {
                    sat.ExportRoutingTable(path);
                }
            }
        }

        /// <summary>
        /// Updates positions of all satellites "ticks" time
        /// </summary>
        /// <param name="ticks">The amount of ticks to advance the simulation by.</param>
        public void PositionTick(int ticks)
        {
            for (var k = 0; k < ticks; k++)
            {
                for (var i = 0; i < _configuration.NumberOfOrbits; i++)
                {
                    var firstLoc = _constellation[i][0].Location; //Copy location of satellite 0
                    var firstLocPos = _constellation[i][0].LogicalPosition; //Copy logical location of satellite 0

                    for (var j = 0; j < _configuration.NumberOfSatellitesPerOrbit; j++)
                    {
                        _constellation[i][j].Location = _constellation[i][(j + 1) % _configuration.NumberOfSatellitesPerOrbit].Location; //Move all satellites one location forward (last becomes equal to zero)
                        _constellation[i][j].LogicalPosition = _constellation[i][(j + 1) % _configuration.NumberOfSatellitesPerOrbit].LogicalPosition; //Move all satellites one logical position forward
                    }

                    _constellation[i][_configuration.NumberOfSatellitesPerOrbit - 1].Location = firstLoc; //Copy saved location zero to new last
                    _constellation[i][_configuration.NumberOfSatellitesPerOrbit - 1].LogicalPosition = firstLocPos; //Copy saved logical location zero to new last
                }
            }
        }

        private readonly SimulationConfiguration _configuration;
        private readonly List<List<Satellite>> _constellation;
    }
}
