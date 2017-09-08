using System.Threading.Tasks;

namespace IoTGame.Controller
{
    public interface IController
    {
        Task StartAsync();
        Task StopAsync();
    }
}