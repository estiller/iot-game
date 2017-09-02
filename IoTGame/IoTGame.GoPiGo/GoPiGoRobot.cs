using System;
using System.Globalization;
using System.Threading.Tasks;
using IoTGame.GoPiGo.Platform;

namespace IoTGame.GoPiGo
{
    public class GoPiGoRobot : IGoPiGoRobot
    {
        private readonly IPlatform _platform;

        public GoPiGoRobot(IPlatform platform)
        {
            _platform = platform;
            Motor = new GoPiGoMotor(platform);
            DistanceSensor = new GoPiGoDistanceSensor(platform);
            Led = new GoPiGoLed(platform);
        }

        public Task OpenAsync()
        {
            return _platform.OpenAsync();
        }

        public async Task<string> GetFirmwareVersionAsync()
        {
            var output = await _platform.WriteReadAsync(Commands.Version, 1);
            return output[0].ToString(CultureInfo.InvariantCulture);
        }

        public async Task<decimal> GetBatteryVoltageAsync()
        {
            var output = await _platform.WriteReadAsync(Commands.BatteryVoltage, 2);
            decimal voltage = 5m * (output[0] * 256 + output[1]) / 1024m / 0.4m;
            return Math.Round(voltage, 2);

        }

        public IMotor Motor { get; }

        public IDistanceSensor DistanceSensor { get; }

        public ILed Led { get; }
    }
}