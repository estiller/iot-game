namespace IoTGame.Driver
{
    public struct Vector
    {
        public Vector(double x, double y)
        {
            X = x;
            Y = y;
        }

        public double X { get; }
        public double Y { get; }
    }
}