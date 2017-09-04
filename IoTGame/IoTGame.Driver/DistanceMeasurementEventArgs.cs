using System;

namespace IoTGame.Driver
{
    public class DistanceMeasurementEventArgs : EventArgs
    {
        public DistanceMeasurementEventArgs(int distance)
        {
            Distance = distance;
        }

        public int Distance { get; }
    }
}