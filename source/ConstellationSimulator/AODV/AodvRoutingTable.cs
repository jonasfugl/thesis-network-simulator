using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ConstellationSimulator.Configuration;
using ConstellationSimulator.Helpers;
using CsvHelper;

namespace ConstellationSimulator.AODV
{
    internal class AodvRoutingTable
    {
        public AodvRoutingTable(string localAddress, Logger logger, SimulationConfiguration conf)
        {
            _table = new Dictionary<string, AodvTableEntry>();
            _logger = logger;
            SequenceNumber = 0;
            _rreqId = 0;
            _time = TimeHelper.GetInstance();
            _localAddress = localAddress;
            _conf = conf;
        }

        /// <summary>
        /// Adds a node address to the list of precusors of an existing routing entry.
        /// </summary>
        /// <param name="destination"></param>
        /// <param name="precursor"></param>
        public void AddPrecursorToEntry(string destination, string precursor)
        {
            _table[destination].Precursors.Add(precursor);
        }

        /// <summary>
        /// Updates the expiration time of an existing routing entry. Note that expiration time also can be delete period.
        /// </summary>
        /// <param name="destination"></param>
        /// <param name="lifetime"></param>
        public void UpdateExpirationTime(string destination, DateTime lifetime)
        {
            _table[destination].ExpirationTime = lifetime;
        }

        /// <summary>
        /// Marks the routing entry for the supplied destination as active.
        /// </summary>
        /// <param name="destination"></param>
        public void SetRouteActive(string destination)
        {
            _table[destination].SetActive(_conf);
        }

        /// <summary>
        /// Invalidates the routing entry for the supplied destination. See the implementation of the routing table, for definition of "invalidates".
        /// </summary>
        /// <param name="destination"></param>
        public void InvalidateEntry(string destination)
        {
            _table[destination].Invalidate(_conf);
            _logger.WriteLine("Invalidating routing entry for " + destination);
        }

        /// <summary>
        /// Updates the destination sequence number of the routing entry, for the supplied destination.
        /// </summary>
        /// <param name="destination">Destination to update DSN for.</param>
        /// <param name="dsn">The new Destination Sequence Number</param>
        public void UpdateDestinationSequenceNumber(string destination, int dsn)
        {
            _table[destination].DestinationSequenceNumber = dsn;
        }

        /// <summary>
        /// Invalidates any routing entries, that has expired. Also deletes routing entries, which has expired and are not active.
        /// </summary>
        public void InvalidateOrRemoveExpiredRoutes()
        {
            foreach (var route in _table.Keys.ToList())
            {
                //If route is active but lifetime has expired
                if (_table[route].Valid && _time.CurrentTime >= _table[route].ExpirationTime)
                {
                    _table[route].Invalidate(_conf);
                    _logger.WriteLine("Invalidating routing entry for " + route + ", because it has expired. New expiration time (delete): " + _table[route].ExpirationTime.ToLongTimeString());
                }
                //If route is not active, and lifetime has expired
                else if (!_table[route].Valid && _time.CurrentTime >= _table[route].ExpirationTime)
                {
                    _logger.WriteLine("Deleting invalid routing entry for " + route + ", because it has expired.");
                    _table.Remove(route); //Delete route pr. section 6.11
                }
            }
        }

        /// <summary>
        /// Called when an RERR is to be sent. It invalidates any routing entries that have broken as a result of the missing neighbour,
        /// and returns a list of destinations now unreachable.
        /// </summary>
        /// <param name="missingNextHop"></param>
        /// <returns>List of destinations that are now unreachable due to the missing next hop</returns>
        public List<UnreachableDestination> HandleAndGetUnreachableDestinations(string missingNextHop)
        {
            var unreachableDestinations = new List<UnreachableDestination>();

            //Per section 6.11 (i) and (ii)
            foreach (var destination in _table.Keys)
            {
                //If this is an entry for a now unreachable destination or the missing neighbour
                if (_table[destination].NextHop == missingNextHop)
                {
                    var ud = new UnreachableDestination(destination, _table[destination].DestinationSequenceNumber);
                    unreachableDestinations.Add(ud);

                    if (_table[destination].Valid)
                    {
                        _table[destination].DestinationSequenceNumber++;
                    }

                    _table[destination].Invalidate(_conf);
                }
            }

            return unreachableDestinations;
        }

        /// <summary>
        /// Exports the routing table to a csv file, if enabled in the configuration.
        /// </summary>
        /// <param name="path"></param>
        public void Print(string path)
        {
            var csvPath = path+"/" + _localAddress + "-routing-table.csv";
            var file = new StreamWriter(csvPath, false) { AutoFlush = true };
            var csv = new CsvWriter(file);

            csv.WriteHeader<AodvTableEntry>();
            csv.NextRecord();
            csv.WriteRecords(_table.Values);
            csv.NextRecord();
            csv.Flush();
            file.Close();
        }

        /// <summary>
        /// Checks if an active route to the destination exists.
        /// </summary>
        /// <param name="destination"></param>
        /// <returns>True if an active route exists, false if no active route exists. If a route does exist, but is not active, false is also returned.</returns>
        public bool ActiveRouteExists(string destination)
        {
            if (_table.ContainsKey(destination))
            {
                return _table[destination].Valid;
            }

            return false;
        }

        /// <summary>
        /// Checks if an route (active or invalid) exists to the destination.
        /// </summary>
        /// <param name="destination"></param>
        /// <returns>True if a route exists, false if it does not.</returns>
        public bool RouteExists(string destination)
        {
            return _table.ContainsKey(destination);
        }

        /// <summary>
        /// Finds the routing table entry for the supplied destination.
        /// </summary>
        /// <param name="destination"></param>
        /// <returns></returns>
        public AodvTableEntry GetEntry(string destination)
        {
            return RouteExists(destination) ? _table[destination] : null;
        }

        /// <summary>
        /// Adds a new routing table entry to the routing table
        /// </summary>
        /// <param name="entry"></param>
        public void AddEntry(AodvTableEntry entry)
        {
            _table[entry.DestinationAddress] = entry;
            LogRouteEntry(entry, false);
        }

        /// <summary>
        /// Updates an existing routing table entry, with the entry provided.
        /// </summary>
        /// <param name="entry"></param>
        public void UpdateEntry(AodvTableEntry entry)
        {
            var destination = entry.DestinationAddress;

            _table[destination].DestinationAddress = entry.DestinationAddress;
            _table[destination].DestinationSequenceNumber = entry.DestinationSequenceNumber;
            _table[destination].ValidDestinationSequenceNumberFlag = entry.ValidDestinationSequenceNumberFlag;
            _table[destination].HopCount = entry.HopCount;
            _table[destination].Valid = entry.Valid;
            _table[destination].NextHop = entry.NextHop;
            _table[destination].Precursors = entry.Precursors.ToList();
            _table[destination].ExpirationTime = entry.ExpirationTime;

            LogRouteEntry(entry, true);
        }

        /// <summary>
        /// Increments the sequence number of this node. Must only be done in certain places, see RFC.
        /// </summary>
        public void IncrementSequenceNumber()
        {
            SequenceNumber = SequenceNumber + 1;
        }

        /// <summary>
        /// Prints the creation or update of a routing entry to the log file.
        /// </summary>
        /// <param name="entry"></param>
        /// <param name="update"></param>
        private void LogRouteEntry(AodvTableEntry entry, bool update)
        {
            var added = update ? "Routing entry updated: " : "Routing entry added: ";

            var msg = "Destination: " + entry.DestinationAddress +
                      ", DSN: " + entry.DestinationSequenceNumber +
                      ", Valid DSN: " + entry.ValidDestinationSequenceNumberFlag +
                      ", Hop Count: " + entry.HopCount +
                      ", Next Hop: " + entry.NextHop +
                      ", Valid: " + entry.Valid +
                      ", ExpirationTime: " + entry.ExpirationTime.ToLongTimeString();

            _logger.WriteLine(added + msg);
        }

        /// <summary>
        /// The Sequence Number for this node.
        /// </summary>
        public int SequenceNumber { get; private set; }

        /// <summary>
        /// An incremental ID used when generating RREQs, to uniquely identify a RREQ from an originator.
        /// </summary>
        public int NextRouteRequestId => ++_rreqId;

        private readonly Dictionary<string, AodvTableEntry> _table;
        private readonly TimeHelper _time;
        private readonly SimulationConfiguration _conf;
        private readonly Logger _logger;
        private int _rreqId;
        private readonly string _localAddress;
    }
}
