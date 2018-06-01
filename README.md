# Network simulator for AODV in Low Earth orbit satellite constellations
A C# simple satellite network simulator, built for a master's thesis in computer engineering.

The purpose of the thesis was to establish a suitable routing protocol for satellite constellations, located in 
Low Earth Orbit. Furthermore, the routing protocol should support a the vision of an Open Internet in Space, where multiple
independent entities can launch nanosatellites, and have them cooperatively form a network, and help each other with connectivity to the Earth.

The simulator was implemented to verify the suitability of AODV as a routing protocol compared to a simple flooding approach. 
AODV is implemented according to [RFC3561](https://tools.ietf.org/html/rfc3561).

## Getting started
1. Open solution in Visual Studio and set SimulatorApp as start-up project.
2. Build solution
3. Find the folder in which the built executable is located (depending on configuration).
4. Run the simulator, to create empty "configs" and "results" folders.
4. Create configuration files for the wanted simulation - inspiration can be found in "configs" in the repo.
5. Place all wanted configuration files within "configs". Samples are provided in "/configs" as mentioned above.
6. Launch the simulator, and wait for it to finish. Results are exported automatically, to the folder specified in the config-file.

## Configuration files
The config-files, formatted in JSON, should be placed in the "configs"-folder, as described above. This file contains the configuration parameters for the set up simulation, that differs from the default parameters.
Samples are provided in the "configs" folder, and for available settings and their default values, the file 
[SimulationConfiguration.cs](https://github.com/jonasfugl/thesis_network_simulator/blob/master/source/ConstellationSimulator/Configuration/SimulationConfiguration.cs)
should be consulted.

Note that a simulation group and simulation name must be provided, as the name of the output folder where the results are stored, is generated from these parameters.

## Unit tests
The project ConstellationSimulator.Test.Unit contains unit-tests for the developed simulator code library. The unit-tests are developed
using NUnit. Not all parts of the code have been tested, due to dependencies that cannot be injected. In the future, the simulator will be 
refactored, to allow unit-testing of more parts of the code.

## Constraints
* Can at the moment only simulate polar constellations (Walker-star)
* Does not take the curvature of the Earth into account, when calculating available links in the neighbour ring.
* The link- and physical layer of the protocol stack have very abstract functionality, and do not emulate their real world counterparts.
