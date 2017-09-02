using System;

namespace IoTGame.Driver
{
    public class MovementEventArgs : EventArgs
    {
        public MovementEventArgs(Direction direction, double x, double y)
        {
            Direction = direction;
            X = x;
            Y = y;
        }

        public Direction Direction { get; }
        public double X { get; }
        public double Y { get; }
    }
}