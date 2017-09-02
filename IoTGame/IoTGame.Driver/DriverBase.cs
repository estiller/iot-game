using System;
using System.Threading.Tasks;
using IoTGame.GoPiGo;

namespace IoTGame.Driver
{
    public abstract class DriverBase : IDriver
    {
        protected IGoPiGoRobot Robot { get; }

        public int SensorAngle { get; protected set; }

        protected DriverBase(IGoPiGoRobot robot)
        {
            Robot = robot;
            SensorAngle = 90;
        }

        public virtual async Task StartAsync()
        {
            await InitSensor();
            await InitLed();
        }

        public virtual async Task StopAsync()
        {
            await Robot.Motor.StopAsync();
            await Robot.DistanceSensor.SetStateAsync(BinaryState.Off);
            await Robot.Led.SetLedAsync(BinaryState.Off);
        }

        public event EventHandler<MovementEventArgs> MovementReadingAvailable;

        protected virtual void OnMovementReadingAvailable(Direction direction, double x, double y)
        {
            var handler = MovementReadingAvailable;
            handler?.Invoke(this, new MovementEventArgs(direction, x, y));
        }

        private async Task InitSensor()
        {
            var distanceSensor = Robot.DistanceSensor;
            await distanceSensor.SetStateAsync(BinaryState.On);
            await distanceSensor.SetAngleAsync(SensorAngle);
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
    }
}