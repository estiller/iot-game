using System.Threading.Tasks;
using IoTGame.GoPiGo.Platform;

namespace IoTGame.GoPiGo
{
    internal class GoPiGoLed : ILed
    {
        private const byte LedPin = 10;
        private readonly ICommandInvoker _commandInvoker;

        internal GoPiGoLed(ICommandInvoker commandInvoker)
        {
            _commandInvoker = commandInvoker;
        }

        public Task OpenAsync()
        {
            return _commandInvoker.WriteAsync(Commands.PinMode, LedPin, (byte) PinMode.Output);
        }

        public Task SetLedAsync(BinaryState state)
        {
            return _commandInvoker.WriteAsync(Commands.DigitalWrite, LedPin, (byte) state);
        }
    }
}