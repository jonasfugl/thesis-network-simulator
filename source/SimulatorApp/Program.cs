using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using ConstellationSimulator;
using ConstellationSimulator.AODV;
using ConstellationSimulator.Configuration;
using ConstellationSimulator.Helpers;
using ConstellationSimulator.Network;
using ConstellationSimulator.Statistics;

namespace SimulatorApp
{
    //Program icon made by Freepik from www.flaticon.com
    //https://www.flaticon.com/free-icon/satellite-station_254009

    class Program
    {
        static void Main(string[] args)
        {
            //Preparing input- and output folders
            var configPath = "configs/";
            var resultPath = "results/";

            Directory.CreateDirectory(configPath);
            Directory.CreateDirectory(resultPath);

            var files = Directory.GetFiles(configPath);

            foreach (var configFile in files)
            {
                var simulator = new Simulator(configFile);
                simulator.Start();
            }
        }
    }
}
