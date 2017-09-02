using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Windows.Gaming.Input;
using IoTGame.Driver;
using IoTGame.GoPiGo;

namespace IoTGame.WinApp.Driver
{
    public class GamepadDriver : DriverBase
    {
        private CancellationTokenSource _cancellationTokenSource;
        private Task _loopTask;
        private Gamepad _gamepad;

        public GamepadDriver(IGoPiGoRobot robot) : base(robot)
        {
        }

        public override async Task StartAsync()
        {
            await base.StartAsync();

            _cancellationTokenSource = new CancellationTokenSource();
            InitGamepad();
            _loopTask = GamepadLoopAsync(_cancellationTokenSource.Token);
        }

        public override async Task StopAsync()
        {
            StopGamepad();
            _cancellationTokenSource.Cancel();
            try
            {
                await _loopTask;
                _loopTask = null;
            }
            catch (TaskCanceledException)
            {
            }

            await base.StopAsync();
        }

        private void InitGamepad()
        {
            Gamepad.GamepadAdded += AddGamepad;
            Gamepad.GamepadRemoved += RemoveGamepad;
            _gamepad = Gamepad.Gamepads.FirstOrDefault();
        }
        private void StopGamepad()
        {
            Gamepad.GamepadAdded -= AddGamepad;
            Gamepad.GamepadRemoved -= RemoveGamepad;
            _gamepad = null;
        }

        private void AddGamepad(object sender, Gamepad gamepad)
        {
            _gamepad = gamepad;
        }
        private void RemoveGamepad(object sender, Gamepad gamepad)
        {
            _gamepad = null;
        }

        private async Task GamepadLoopAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                await Task.Delay(50, cancellationToken);
                if (_gamepad == null || cancellationToken.IsCancellationRequested)
                    continue;

                var reading = _gamepad.GetCurrentReading();
                await MoveRobotAsync(reading);
                await MoveSensorAsync(reading);
            }
        }

        private async Task MoveRobotAsync(GamepadReading reading)
        {
            var direction = VectorDirection(reading.LeftThumbstickX, reading.LeftThumbstickY);
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

        private async Task MoveSensorAsync(GamepadReading reading)
        {
            var direction = VectorDirection(reading.RightThumbstickX, reading.RightThumbstickY);
            switch (direction)
            {
                case Direction.Stop:
                case Direction.Forward:
                case Direction.Backward:
                    break;
                case Direction.Right:
                    SensorAngle -= 3;
                    await Robot.DistanceSensor.SetAngleAsync(SensorAngle);
                    break;
                case Direction.Left:
                    SensorAngle += 3;
                    await Robot.DistanceSensor.SetAngleAsync(SensorAngle);
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

        private enum Direction
        {
            Stop,
            Forward,
            Backward,
            Right,
            Left
        }
    }
}