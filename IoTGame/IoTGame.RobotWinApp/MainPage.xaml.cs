using System;
using System.Globalization;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using IoTGame.Controller;
using IoTGame.Driver;
using IoTGame.GoPiGo;
using IoTGame.RobotWinApp.Controller;
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
            var goPiGoDriver = new GoPiGoDriver(robot);
            var driver = new EventDriverDecorator(goPiGoDriver);
            driver.DriveCommandAvailable += DriveCommandAvailable;
            _controller = new GamepadController(driver);

            _updateTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            _updateTimer.Tick += OnUpdateTimerTick;

            InitializeComponent();


            void DriveCommandAvailable(object sender, DriveCommandEventArgs args)
            {
                VelocityGraph.VectorX = args.Command.MotionVector.X;
                VelocityGraph.VectorY = args.Command.MotionVector.Y;
            }

            async void OnUpdateTimerTick(object sender, object args)
            {
                if (string.IsNullOrEmpty(FirmwareVersionText.Text))
                    FirmwareVersionText.Text = await robot.GetFirmwareVersionAsync();
                BatteryVoltageText.Text = (await robot.GetBatteryVoltageAsync()).ToString(CultureInfo.CurrentCulture);
                DistanceText.Text = (await robot.DistanceSensor.MeasureInCentimetersAsync()).ToString(CultureInfo.CurrentCulture);
            }
        }

        private async void RobotState_Checked(object sender, RoutedEventArgs e)
        {
            if (!(sender is RadioButton radioButton))
                return;

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
            _updateTimer.Start();
        }

        private async Task StopRobot()
        {
            _updateTimer.Stop();
            await _controller.StopAsync();
        }
    }
}
