using System;

namespace IoTGame.Driver
{
    public class ReportBackEventArgs : EventArgs
    {
        public ReportBackEventArgs(int distanceCm, decimal voltage)
        {
            DistanceCm = distanceCm;
            Voltage = voltage;
        }

        public int DistanceCm { get; }
        public decimal Voltage { get; }
    }
}