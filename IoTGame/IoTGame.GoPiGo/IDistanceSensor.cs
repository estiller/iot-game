using System.Threading.Tasks;

namespace IoTGame.GoPiGo
{
    public interface IDistanceSensor
    {
        Task SetStateAsync(BinaryState state);
        Task SetAngleAsync(int degrees);
        Task<int> MeasureInCentimetersAsync();
    }
}