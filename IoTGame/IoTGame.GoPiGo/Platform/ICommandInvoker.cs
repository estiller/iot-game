using System.Threading.Tasks;

namespace IoTGame.GoPiGo.Platform
{
    public interface ICommandInvoker
    {
        Task WriteAsync(byte command, byte firstParam = 0, byte secondParam = 0, byte thirdParam = 0);

        Task<byte[]> WriteReadAsync(byte command, int bytesToRead, byte firstParam = 0, byte secondParam = 0, byte thirdParam = 0);
    }
}