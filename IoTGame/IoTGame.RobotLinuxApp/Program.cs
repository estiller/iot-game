using System;
using System.Globalization;
using System.Timers;
using IoTGame.Constants;
using IoTGame.GoPiGo;
using IoTGame.RobotLinuxApp.GoPiGo;
using IoTGame.Driver;
using IoTGame.Controller;

namespace IoTGame.RobotLinuxApp
{
    class Program
    {
        static void Main(string[] args)
        {
            var robot = new GoPiGoRobot(new LinuxIoTPlatform());
            var driver = new GoPiGoDriver(robot);
            var playerFilter = new PlayerFilterDecorator(PlayerIds.Red, driver);
            var controller = new ServiceBusController(PlayerIds.Red, playerFilter, ServiceBusConstants.DeviceSubscriptionName);

            var updateTimer = new Timer(1000) {AutoReset = true};
            updateTimer.Elapsed += async (sender, eventArgs) =>
            {
                var voltage = await robot.GetBatteryVoltageAsync();
                Console.WriteLine($"Voltage:  {voltage.ToString(CultureInfo.CurrentCulture)}");

                var distanceCm = await robot.DistanceSensor.MeasureInCentimetersAsync();
                Console.WriteLine($"Distance: {distanceCm.ToString(CultureInfo.CurrentCulture)}");

                Console.WriteLine();
                await controller.ReportBackAsync(distanceCm, voltage);
            };

            Console.WriteLine("Opening Robot");
            controller.StartAsync().GetAwaiter().GetResult();
            updateTimer.Enabled = true;
            Console.WriteLine("Listening. Press <enter> to exit");
            Console.ReadLine();
            controller.StopAsync().GetAwaiter().GetResult();
        }
    }
}
