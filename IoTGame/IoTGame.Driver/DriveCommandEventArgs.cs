using System;

namespace IoTGame.Driver
{
    public class DriveCommandEventArgs : EventArgs
    {
        public DriveCommand Command { get; }

        public DriveCommandEventArgs(DriveCommand command)
        {
            Command = command;
        }
    }
}