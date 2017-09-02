using System.Threading.Tasks;
using IoTGame.GoPiGo.Platform;

namespace IoTGame.GoPiGo
{
    internal class GoPiGoMotor : IMotor
    {
        private readonly ICommandInvoker _commandInvoker;

        internal GoPiGoMotor(ICommandInvoker commandInvoker)
        {
            _commandInvoker = commandInvoker;
        }

        public Task MoveForwardAsync()
        {
            return _commandInvoker.WriteAsync(Commands.MoveForward);
        }

        public Task MoveBackwardAsync()
        {
            return _commandInvoker.WriteAsync(Commands.MoveBackward);
        }

        public Task MoveLeftAsync()
        {
            return _commandInvoker.WriteAsync(Commands.MoveLeft);
        }

        public Task MoveRightAsync()
        {
            return _commandInvoker.WriteAsync(Commands.MoveRight);
        }

        public Task StopAsync()
        {
            return _commandInvoker.WriteAsync(Commands.Stop);
        }
    }
}