using System.IO;
using Newtonsoft.Json;

namespace ConstellationSimulator.Configuration
{
    public class SimulationConfigurationLoader
    {
        public static SimulationConfiguration LoadConfiguration(string path)
        {
            var conf = JsonConvert.DeserializeObject<SimulationConfiguration>(File.ReadAllText(path));
            conf.Initialize();

            return conf;
        }
    }
}
