using System.Threading.Tasks;

namespace IoTGame.GoPiGo
{
    public interface IGoPiGoRobot
    {
        Task OpenAsync();

        Task<string> GetFirmwareVersionAsync();
        Task<decimal> GetBatteryVoltageAsync();

        IMotor Motor { get; }

        IDistanceSensor DistanceSensor { get; }

        ILed Led { get; }
    }
}