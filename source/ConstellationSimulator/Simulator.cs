using System;
using System.Collections.Generic;
using System.IO;
using ConstellationSimulator.Configuration;
using ConstellationSimulator.Helpers;
using ConstellationSimulator.Statistics;
using CsvHelper;

namespace ConstellationSimulator
{
    public class Simulator
    {
        public Simulator(string configPath)
        {
            _conf = SimulationConfigurationLoader.LoadConfiguration(configPath);
            _statistics = new StatisticsCollector();
            _constellation = new Constellation(_conf, _statistics);
        }

        /// <summary>
        /// Start the simulation. Results are automatically exported at the end of the simulation.
        /// Will run for the specified amount of simulation iterations.
        /// </summary>
        public void Start()
        {
            var simulationPath = "results/" + _conf.SimulationGroup + "/" + _conf.SimulationName;

            for (var i = 1; i <= _conf.NumberOfSimulationIterations; i++)
            {
                _statistics.NewSimulation();

                _conf.OutputPath = "results/" + _conf.SimulationGroup + "/" + _conf.SimulationName + "/run " + i;
                Directory.CreateDirectory(_conf.OutputPath);

                Console.WriteLine("Running " + _conf.SimulationName + ", iteration " + i + " of " + _conf.NumberOfSimulationIterations);

                var result = Run();

                ExportResults(_conf.OutputPath, result);
            }

            ResultOutputter.DumpResults(_statistics.GetResults(), simulationPath, "average-statistics.txt");
        }


        /// <summary>
        /// Setup and starts a simulation.
        /// </summary>
        /// <returns>Result of the simulation</returns>
        private SimulationResult Run()
        {
            var time = TimeHelper.GetInstance();
            time.ResetTime();

            double tickCounter = 1;
            var positionChangeTickCounter = 1;
            uint lastRreqGenerate = 0;

            _statistics.SetStartTime();

            using (var progress = new ProgressBar())
            {
                while (tickCounter < _conf.TotalSimulationTicks)
                {
                    progress.Report(tickCounter / Convert.ToDouble(_conf.TotalSimulationTicks));

                    _constellation.Tick(); //T = n seconds
                    time.Tick(_conf.SecondsPerTick); //T = n + secondsPerTick

                    if (positionChangeTickCounter == _conf.TicksPerPositionChange)
                    {
                        positionChangeTickCounter = 0;
                        _constellation.PositionTick(1);
                    }

                    //Only save snapshot information, if required
                    if (_conf.ExportSnapshots)
                    {
                        var results = _statistics.GetCurrentResults();

                        var snapshot = new StatisticSnapshot
                        {
                            MinutesIntoSim = (Convert.ToDouble(tickCounter) * Convert.ToDouble(_conf.SecondsPerTick) / 60).ToString("N0"),
                            DataPacketDeliveryRate = Convert.ToDouble(results.TotalDataPacketsReceived) / Convert.ToDouble(results.TotalDataPacketsSent),
                            Goodput = Convert.ToDouble(results.TotalDataPacketsReceived) / Convert.ToDouble(results.TotalPacketsReceivedNetwork),
                            GeneratedRreqs = Convert.ToUInt32(results.TotalRoutingRequestsGenerated) - lastRreqGenerate
                        };

                        lastRreqGenerate = Convert.ToUInt32(results.TotalRoutingRequestsGenerated);
                        _statisticSnapshots.Add(snapshot);
                    }

                    positionChangeTickCounter++;
                    tickCounter++;
                }
            }

            _statistics.SetStopTime();

            return _statistics.GetCurrentResults();
        }

        /// <summary>
        /// Exports the results to the specified path.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="result"></param>
        private void ExportResults(string path, SimulationResult result)
        {
            //Dump snapshots if enabled
            if (_conf.ExportSnapshots)
            {
                var snapshotsFile = new StreamWriter(path + "/snapshots.csv", false);
                var csv = new CsvWriter(snapshotsFile);
                csv.Configuration.Delimiter = ";";
                csv.WriteHeader<StatisticSnapshot>();
                csv.NextRecord();
                csv.WriteRecords(_statisticSnapshots);
                csv.NextRecord();
                csv.Flush();
                snapshotsFile.Flush();
            }

            //Dump routing tables if enabled
            if (_conf.EnableDumpRoutingTables)
            {
                _constellation.PrintRoutingTables(path);
            }
            
            ResultOutputter.DumpResults(new List<SimulationResult>{ result }, path, "/statistics.txt");
            _conf.ExportConfiguration(path);
        }

        private readonly List<StatisticSnapshot> _statisticSnapshots = new List<StatisticSnapshot>();
        private readonly StatisticsCollector _statistics;
        private readonly Constellation _constellation;
        private readonly SimulationConfiguration _conf;
    }
}
