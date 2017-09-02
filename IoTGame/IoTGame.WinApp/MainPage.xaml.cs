using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.UserDataTasks;
using Windows.Gaming.Input;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using IoTGame.GoPiGo;
using IoTGame.WinApp.GoPiGo;


namespace IoTGame.WinApp
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private Task _t;

        private int _servoAngle = 90;

        public MainPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            _t = Task.Run(async () =>
            {
                Gamepad gamepad = null;
                Gamepad.GamepadAdded += (sender, g) => gamepad = g;
                Gamepad.GamepadRemoved += (sender, g) => gamepad = null;
                gamepad = Gamepad.Gamepads.FirstOrDefault();

                var gopigo = new GoPiGoRobot(new WindowsIoTPlatform());
                await gopigo.OpenAsync();

                var voltage = await gopigo.GetBatteryVoltageAsync();
                await Task.Delay(100);

                await gopigo.DistanceSensor.SetStateAsync(BinaryState.On);
                await Task.Delay(100);
                await gopigo.DistanceSensor.SetAngleAsync(_servoAngle);
                await Task.Delay(100);

                await gopigo.Led.OpenAsync();
                for (int i = 0; i < 3; i++)
                {
                    await gopigo.Led.SetLedAsync(BinaryState.Off);
                    await Task.Delay(100);
                    await gopigo.Led.SetLedAsync(BinaryState.On);
                    await Task.Delay(100);
                }

                while (true)
                {
                    await Task.Delay(50);
                    if (gamepad == null)
                        continue;

                    var reading = gamepad.GetCurrentReading();
                    var power = Math.Sqrt(reading.LeftThumbstickX * reading.LeftThumbstickX + reading.LeftThumbstickY * reading.LeftThumbstickY);
                    if (power < 0.2)
                    {
                        await gopigo.Motor.StopAsync();
                    }
                    else
                    {
                        var angle = Math.Atan2(reading.LeftThumbstickY, reading.LeftThumbstickX);
                        if (Math.PI * -0.25 <= angle && angle <= Math.PI * 0.25)
                            await gopigo.Motor.MoveRightAsync();
                        else if (Math.PI * 0.25 <= angle && angle <= Math.PI * 0.75)
                            await gopigo.Motor.MoveForwardAsync();
                        else if (Math.PI * -0.75 <= angle && angle <= Math.PI * -0.25)
                            await gopigo.Motor.MoveBackwardAsync();
                        else
                            await gopigo.Motor.MoveLeftAsync();
                    }

                    power = Math.Sqrt(reading.RightThumbstickX * reading.RightThumbstickX + reading.RightThumbstickY * reading.RightThumbstickY);
                    if (power >= 0.2)
                    {
                        var angle = Math.Atan2(reading.RightThumbstickY, reading.RightThumbstickX);
                        if (Math.PI * -0.25 <= angle && angle <= Math.PI * 0.25)
                        {
                            _servoAngle -= 3;
                            await gopigo.DistanceSensor.SetAngleAsync(_servoAngle);
                        }
                        else if (Math.PI * 0.75 <= angle || angle <= Math.PI * -0.75)
                        {
                            _servoAngle += 3;
                            await gopigo.DistanceSensor.SetAngleAsync(_servoAngle);
                        }
                    }

                    int cm = await gopigo.DistanceSensor.MeasureInCentimetersAsync();
                }
            });
        }
    }
}
