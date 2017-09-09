using System;
using System.Globalization;
using System.Threading.Tasks;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using IoTGame.Constants;
using IoTGame.Controller;
using IoTGame.Driver;
using IoTGame.GoPiGo;
using IoTGame.RobotWinApp.GoPiGo;

namespace IoTGame.RobotWinApp
{
    public sealed partial class MainPage
    {
        private readonly IController _controller;
        private readonly DispatcherTimer _updateTimer;

        public MainPage()
        {
            var robot = new GoPiGoRobot(new WindowsIoTPlatform());
            var driver = new GoPiGoDriver(robot);
            var eventDecorator = new EventDriverDecorator(driver);
            eventDecorator.DriveCommandAvailable += DriveCommandAvailable;
            //_controller = new GamepadController(PlayerIds.White, eventDecorator);
            var playerFilter = new PlayerFilterDecorator(PlayerIds.White, eventDecorator);
            var serviceBusController = new ServiceBusController(playerFilter, ServiceBusConstants.DeviceSubscriptionName);
            _controller = serviceBusController;

            _updateTimer = new DispatcherTimer {Interval = TimeSpan.FromSeconds(1)};
            _updateTimer.Tick += async (sender, args) =>
            {
                if (string.IsNullOrEmpty(FirmwareVersionText.Text))
                    FirmwareVersionText.Text = await robot.GetFirmwareVersionAsync();

                var voltage = await robot.GetBatteryVoltageAsync();
                BatteryVoltageText.Text = voltage.ToString(CultureInfo.CurrentCulture);

                var distanceCm = await robot.DistanceSensor.MeasureInCentimetersAsync();
                DistanceText.Text = distanceCm.ToString(CultureInfo.CurrentCulture);

                await serviceBusController.ReportBackAsync(distanceCm, voltage);
            };

            InitializeComponent();
        }
        private async void DriveCommandAvailable(object sender, DriveCommandEventArgs args)
        {
            if (!Dispatcher.HasThreadAccess)
            {
                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => DriveCommandAvailable(sender, args));
                return;
            }

            VelocityGraph.VectorX = args.Command.MotionVector.X;
            VelocityGraph.VectorY = args.Command.MotionVector.Y;
        }

        private async void RobotState_Checked(object sender, RoutedEventArgs e)
        {
            var radioButton = (RadioButton) sender;
            string state = radioButton.Tag.ToString();
            switch (state)
            {
                case "On":
                    await StartRobot();
                    break;
                case "Off":
                    await StopRobot();
                    break;
            }
        }

        private async Task StartRobot()
        {
            await _controller.StartAsync();
            _updateTimer?.Start();
        }

        private async Task StopRobot()
        {
            _updateTimer?.Stop();
            await _controller.StopAsync();
        }
    }
}
