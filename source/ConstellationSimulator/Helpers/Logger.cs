using System.IO;
using ConstellationSimulator.Configuration;

namespace ConstellationSimulator.Helpers
{
    internal class Logger
    {
        public Logger(string path, SimulationConfiguration conf)
        {
            if (conf.EnableSatelliteLogging)
            {
                _file = new StreamWriter(path, false);
            }
            
            _time = TimeHelper.GetInstance();
            _conf = conf;
        }

        /// <summary>
        /// Writes a line to the logfile, if enabled by the configuration.
        /// </summary>
        /// <param name="line"></param>
        public void WriteLine(string line)
        {
            if (_conf.EnableSatelliteLogging)
            {
                _file.WriteLine(_entryCounter + "--" + _time.CurrentTime.ToLongTimeString() + ": " + line);
                _entryCounter++;
            }
        }

        private readonly SimulationConfiguration _conf;
        private int _entryCounter = 1;
        private readonly TimeHelper _time;
        private readonly StreamWriter _file;
    }
}
