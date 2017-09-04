using System;
using System.Threading.Tasks;
using IoTGame.GoPiGo;

namespace IoTGame.Driver
{
    public class GoPiGoDriver : IGoPiGoDriver
    {
        private int _sensorAngle;

        public GoPiGoDriver(IGoPiGoRobot robot)
        {
            Robot = robot;
            _sensorAngle = 90;
        }

        public IGoPiGoRobot Robot { get; }

        public virtual async Task StartAsync()
        {
            await Robot.OpenAsync();
            await InitSensor();
            await InitLed();
        }

        public async Task DriveAsync(DriveCommand command)
        {
            await MoveRobotAsync(command.MotionVector);
            await MoveSensorAsync(command.SensorVector);
        }

        public virtual async Task StopAsync()
        {
            await Robot.Motor.StopAsync();
            await Robot.DistanceSensor.SetStateAsync(BinaryState.Off);
            await Robot.Led.SetLedAsync(BinaryState.Off);
        }

        private async Task InitSensor()
        {
            var distanceSensor = Robot.DistanceSensor;
            await distanceSensor.SetStateAsync(BinaryState.On);
            await distanceSensor.SetAngleAsync(_sensorAngle);
        }

        private async Task InitLed()
        {
            var led = Robot.Led;
            await led.OpenAsync();
            for (int i = 0; i < 3; i++)
            {
                await led.SetLedAsync(BinaryState.Off);
                await Task.Delay(100);
                await led.SetLedAsync(BinaryState.On);
                await Task.Delay(100);
            }
        }

        private async Task MoveRobotAsync(Vector motionVector)
        {
            var direction = VectorDirection(motionVector.X, motionVector.Y);
            switch (direction)
            {
                case Direction.Stop:
                    await Robot.Motor.StopAsync();
                    break;
                case Direction.Forward:
                    await Robot.Motor.MoveForwardAsync();
                    break;
                case Direction.Backward:
                    await Robot.Motor.MoveBackwardAsync();
                    break;
                case Direction.Right:
                    await Robot.Motor.MoveRightAsync();
                    break;
                case Direction.Left:
                    await Robot.Motor.MoveLeftAsync();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private async Task MoveSensorAsync(Vector sensorVector)
        {
            var direction = VectorDirection(sensorVector.X, sensorVector.Y);
            switch (direction)
            {
                case Direction.Stop:
                case Direction.Forward:
                case Direction.Backward:
                    break;
                case Direction.Right:
                    _sensorAngle -= 3;
                    await Robot.DistanceSensor.SetAngleAsync(_sensorAngle);
                    break;
                case Direction.Left:
                    _sensorAngle += 3;
                    await Robot.DistanceSensor.SetAngleAsync(_sensorAngle);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static double VectorLength(double x, double y)
        {
            return Math.Sqrt(x * x + y * y);
        }
        private static double VectorAngle(double x, double y)
        {
            return Math.Atan2(y, x);
        }

        private static Direction VectorDirection(double x, double y)
        {
            const double deadzoneThreshold = 0.2;

            var vectorLength = VectorLength(x, y);
            if (vectorLength < deadzoneThreshold)
            {
                return Direction.Stop;
            }

            var angle = VectorAngle(x, y);
            if (Math.PI * -0.25 <= angle && angle <= Math.PI * 0.25)
                return Direction.Right;
            if (Math.PI * 0.25 <= angle && angle <= Math.PI * 0.75)
                return Direction.Forward;
            if (Math.PI * -0.75 <= angle && angle <= Math.PI * -0.25)
                return Direction.Backward;
            return Direction.Left;
        }
    }
}