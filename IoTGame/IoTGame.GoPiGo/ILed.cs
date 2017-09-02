using System.Threading.Tasks;

namespace IoTGame.GoPiGo
{
    public interface ILed
    {
        Task OpenAsync();
        Task SetLedAsync(BinaryState state);
    }
}