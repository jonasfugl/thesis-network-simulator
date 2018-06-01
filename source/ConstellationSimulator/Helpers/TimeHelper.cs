using System;

namespace ConstellationSimulator.Helpers
{
    /// <summary>
    /// Representation of time in the simulator. Implemented as Singleton, to ensure that only one time instance exists, and therefore synchronization of all timestamps.
    /// </summary>
    internal class TimeHelper
    {
        private TimeHelper()
        {
            ResetTime();
        }

        private static TimeHelper _instance;

        public static TimeHelper GetInstance()
        {
            if (_instance == null)
            {
                _instance = new TimeHelper();
            }

            return _instance;
        }

        /// <summary>
        /// Rests the timestamp, needed when starting a new simulation.
        /// </summary>
        public void ResetTime()
        {
            CurrentTime = DateTime.Now;
        }

        /// <summary>
        /// The current time-stamp.
        /// </summary>
        public DateTime CurrentTime { get; private set; }

        /// <summary>
        /// Moves the timestamp forward by the specified amount of seconds.
        /// </summary>
        /// <param name="seconds">The number of seconds to increase the time by.</param>
        public void Tick(int seconds)
        {
            CurrentTime = CurrentTime.AddSeconds(seconds);
        }

        /// <summary>
        /// Calculates a timestamp in the future, used to get timestamps for expiration times.
        /// </summary>
        /// <param name="secondsInFuture">Number of seconds into the future, the timestamp should be, provided as an int.</param>
        /// <returns>The future timestamp.</returns>
        public DateTime GetFutureTime(int secondsInFuture)
        {
            return CurrentTime.AddSeconds(secondsInFuture);
        }

        /// <summary>
        /// Calculates a timestamp in the future, used to get timestamps for expiration times.
        /// </summary>
        /// <param name="secondsInFuture">Number of seconds into the future, the timestamp should be, provided as an double.</param>
        /// <returns>The future timestamp.</returns>
        public DateTime GetFutureTime(double secondsInFuture)
        {
            return GetFutureTime(Convert.ToInt32(secondsInFuture));
        }
    }
}
