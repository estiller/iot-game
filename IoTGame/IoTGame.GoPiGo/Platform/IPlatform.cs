using System.Threading.Tasks;

namespace IoTGame.GoPiGo.Platform
{
    public interface IPlatform : ICommandInvoker
    {
        Task OpenAsync();
    }
}