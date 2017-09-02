using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using IoTGame.Driver;
using IoTGame.GoPiGo;
using IoTGame.WinApp.Driver;
using IoTGame.WinApp.GoPiGo;


namespace IoTGame.WinApp
{
    public sealed partial class MainPage
    {
        private readonly IGoPiGoRobot _robot;
        private readonly IDriver _driver;

        public MainPage()
        {
            _robot = new GoPiGoRobot(new WindowsIoTPlatform());
            _driver = new GamepadDriver(_robot);

            InitializeComponent();
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
        }

        private async Task StopRobot()
        {
            await _driver.StopAsync();
        }
    }
}
