using System.Threading.Tasks;
using IoTGame.GoPiGo;

namespace IoTGame.Driver
{
    public interface IGoPiGoDriver
    {
        IGoPiGoRobot Robot { get; }

        Task StartAsync();
        Task DriveAsync(DriveCommand drive);
        Task StopAsync();
    }
}
