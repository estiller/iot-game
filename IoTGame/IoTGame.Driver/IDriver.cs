using System.Threading.Tasks;

namespace IoTGame.Driver
{
    public interface IDriver
    {
        Task StartAsync();
        Task StopAsync();
    }
}
