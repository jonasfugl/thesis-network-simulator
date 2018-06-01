using System;

namespace ConstellationSimulator.Helpers
{
    internal class RandomHelper
    {
        static readonly Random Random = new Random();

        public static int GetRandomNumber(int min, int max)
        {
            return Random.Next(min, max);
        }

        public static bool GenerateData(int probabilityInPercent)
        {
            return ProbabilityCalculator(probabilityInPercent);
        }

        public static bool ReceiveData(int probabilityInPercent)
        {
            return ProbabilityCalculator(probabilityInPercent);
        }

        private static bool ProbabilityCalculator(int probability)
        {
            if (probability >= 100)
            {
                return true;
            }

            var res = Random.Next(1, 101);
            return res < probability;
        }
    }
}
