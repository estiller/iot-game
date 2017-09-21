using System;

namespace IoTGame.Driver
{
    public class ReportBackEventArgs : EventArgs
    {
        public ReportBackEventArgs(string playerId, int distanceCm, decimal voltage)
        {
            PlayerId = playerId;
            DistanceCm = distanceCm;
            Voltage = voltage;
        }

        public string PlayerId { get; set; }
        public int DistanceCm { get; }
        public decimal Voltage { get; }
    }
}