using System;
using System.Globalization;
using System.Threading.Tasks;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using IoTGame.Driver;
using IoTGame.GoPiGo;
using IoTGame.WinApp.Controller;
using IoTGame.WinApp.GoPiGo;


namespace IoTGame.WinApp
{
    public sealed partial class MainPage
    {
        private readonly GamepadController _controller;

        public MainPage()
        {
            var robot = new GoPiGoRobot(new WindowsIoTPlatform());
            var driver = new GoPiGoDriver(robot);
            var eventDecorator = new EventDriverDecorator(driver);
            var distanceMeasure = new DistanceMeasurementDriver(eventDecorator, TimeSpan.FromSeconds(1));
            _controller = new GamepadController(distanceMeasure);

            eventDecorator.DriveCommandAvailable += DriveCommandAvailable;
            distanceMeasure.DistanceMeasurementAvailable += DistanceMeasurementAvailable;

            InitializeComponent();
        }

        private async void DistanceMeasurementAvailable(object sender, DistanceMeasurementEventArgs e)
        {
            if (!Dispatcher.HasThreadAccess)
            {
                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => DistanceMeasurementAvailable(sender, e));
                return;
            }
            DistanceText.Text = e.Distance.ToString(CultureInfo.InvariantCulture);
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
            await _controller.StartAsync();
        }

        private async Task StopRobot()
        {
            await _controller.StopAsync();
        }
    }
}
