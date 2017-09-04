using System.Threading.Tasks;

namespace IoTGame.Driver
{
    public interface IDriver
    {
        Task StartAsync();
        Task DriveAsync(DriveCommand drive);
        Task StopAsync();
    }
}
