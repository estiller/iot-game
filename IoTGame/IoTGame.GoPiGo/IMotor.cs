using System.Threading.Tasks;

namespace IoTGame.GoPiGo
{
    public interface IMotor
    {
        Task MoveForwardAsync();
        Task MoveBackwardAsync();
        Task MoveLeftAsync();
        Task MoveRightAsync();
        Task StopAsync();
    }
}