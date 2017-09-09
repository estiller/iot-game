using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Windows.Gaming.Input;
using IoTGame.Controller;
using IoTGame.Driver;

namespace IoTGame.AppCommon.Controller
{
    public class GamepadController : IController
    {
        private readonly IDriver _driver;

        private CancellationTokenSource _cancellationTokenSource;
        private Task _loopTask;
        private Gamepad _gamepad;

        public GamepadController(IDriver driver)
        {
            _driver = driver;
        }

        public async Task StartAsync()
        {
            await _driver.StartAsync();

            _cancellationTokenSource = new CancellationTokenSource();
            InitGamepad();
            _loopTask = GamepadLoopAsync(_cancellationTokenSource.Token);
        }

        public async Task StopAsync()
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

            await _driver.StopAsync();
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
                await _driver.DriveAsync(new DriveCommand
                {
                    MotionVector = new Vector(reading.LeftThumbstickX, reading.LeftThumbstickY),
                    SensorVector = new Vector(reading.RightThumbstickX, reading.RightThumbstickY)
                });
            }
        }
    }
}