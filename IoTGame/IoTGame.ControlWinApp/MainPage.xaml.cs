using System;
using System.Globalization;
using System.Threading.Tasks;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using IoTGame.AppCommon.Controller;
using IoTGame.Constants;
using IoTGame.ControlWinApp.IoTHub;
using IoTGame.Driver;

namespace IoTGame.ControlWinApp
{
    public sealed partial class MainPage
    {
        private readonly GamepadController _controller;

        public MainPage()
        {
            //var driver = new ServiceBusDriver();
            var driver = new IoTHubDriver();
            driver.ReportBackAvailable += OnReportBackAvailable;
            var eventDecorator = new EventDriverDecorator(driver);
            eventDecorator.DriveCommandAvailable += DriveCommandAvailable;
            _controller = new GamepadController(PlayerIds.White, eventDecorator);

            InitializeComponent();
        }

        private async void OnReportBackAvailable(object sender, ReportBackEventArgs args)
        {
            if (args.PlayerId != _controller.PlayerId)
                return;

            if (!Dispatcher.HasThreadAccess)
            {
                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => OnReportBackAvailable(sender, args));
                return;
            }

            BatteryVoltageText.Text = args.Voltage.ToString(CultureInfo.CurrentCulture);
            DistanceText.Text = args.DistanceCm.ToString(CultureInfo.CurrentCulture);
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

        private async void AppState_Checked(object sender, RoutedEventArgs e)
        {
            var radioButton = (RadioButton)sender;
            string state = radioButton.Tag.ToString();
            switch (state)
            {
                case "On":
                    await StartApp();
                    break;
                case "Off":
                    await StopApp();
                    break;
            }
        }

        private async Task StartApp()
        {
            await _controller.StartAsync();
        }

        private async Task StopApp()
        {
            await _controller.StopAsync();
        }

        private void Player_Checked(object sender, RoutedEventArgs e)
        {
            var radioButton = (RadioButton)sender;
            string state = radioButton.Tag.ToString();
            switch (state)
            {
                case "White":
                    _controller.PlayerId = PlayerIds.White;
                    break;
                case "Red":
                    _controller.PlayerId = PlayerIds.Red;
                    break;
            }
        }
    }
}
