using System;
using System.Threading.Tasks;
using IoTGame.GoPiGo.Platform;

namespace IoTGame.GoPiGo
{
    internal class GoPiGoDistanceSensor : IDistanceSensor
    {
        private const byte SensorPin = 15;
        private readonly ICommandInvoker _commandInvoker;

        internal GoPiGoDistanceSensor(ICommandInvoker commandInvoker)
        {
            _commandInvoker = commandInvoker;
        }

        public Task SetStateAsync(BinaryState state)
        {
            switch (state)
            {
                case BinaryState.Off:
                    return _commandInvoker.WriteAsync(Commands.DisableServo);
                case BinaryState.On:
                    return _commandInvoker.WriteAsync(Commands.EnableServo);
                default:
                    throw new ArgumentOutOfRangeException(nameof(state), state, null);
            }
        }

        public Task SetAngleAsync(int degrees)
        {
            return _commandInvoker.WriteAsync(Commands.RotateServo, (byte) degrees);
        }

        public async Task<int> MeasureInCentimetersAsync()
        {
            var output = await _commandInvoker.WriteReadAsync(Commands.UltraSonic, 2, SensorPin);
            return output[0] * 256 + output[1];
        }
    }
}