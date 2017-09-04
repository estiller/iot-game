using System;
using System.Globalization;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using IoTGame.Driver;
using IoTGame.GoPiGo;
using IoTGame.WinApp.GoPiGo;


namespace IoTGame.WinApp
{
    public sealed partial class MainPage
    {
        private readonly IGoPiGoRobot _robot;
        private readonly IDriver _driver;
        private readonly GamepadController _controller;
        private readonly DispatcherTimer _distanceTimer;

        public MainPage()
        {
            _robot = new GoPiGoRobot(new WindowsIoTPlatform());
            _driver = new GoPiGoDriver(_robot);
            var eventDecorator = new EventDriverDecorator(_driver);
            _controller = new GamepadController(eventDecorator);

            eventDecorator.DriveCommandAvailable += DriveCommandAvailable;

            _distanceTimer = new DispatcherTimer {Interval = TimeSpan.FromSeconds(1)};
            _distanceTimer.Tick += MeasureDistance;

            InitializeComponent();
        }

        private async void MeasureDistance(object sender, object o)
        {
            var distance = await _robot.DistanceSensor.MeasureInCentimetersAsync();
            DistanceText.Text = distance.ToString(CultureInfo.InvariantCulture);
        }

        private void DriveCommandAvailable(object sender, DriveCommandEventArgs args)
        {
            VelocityGraph.VectorX = args.Command.MotionVector.X;
            VelocityGraph.VectorY = args.Command.MotionVector.Y;
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
            await _robot.OpenAsync();
            await _driver.StartAsync();
            await _controller.StartAsync();
            _distanceTimer.Start();
        }

        private async Task StopRobot()
        {
            _distanceTimer.Stop();
            await _controller.StopAsync();
            await _driver.StopAsync();
        }
    }
}
