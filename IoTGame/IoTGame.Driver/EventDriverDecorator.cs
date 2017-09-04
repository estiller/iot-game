using System;
using System.Threading.Tasks;

namespace IoTGame.Driver
{
    public class EventDriverDecorator : IDriver
    {
        private readonly IDriver _internalDriver;

        public EventDriverDecorator(IDriver internalDriver)
        {
            _internalDriver = internalDriver;
        }

        public Task StartAsync()
        {
            return _internalDriver.StartAsync();
        }

        public Task DriveAsync(DriveCommand command)
        {
            OnDriveCommandAvailable(command);
            return _internalDriver.DriveAsync(command);
        }

        public Task StopAsync()
        {
            return _internalDriver.StopAsync();
        }

        public event EventHandler<DriveCommandEventArgs> DriveCommandAvailable;

        protected virtual void OnDriveCommandAvailable(DriveCommand command)
        {
            var handler = DriveCommandAvailable;
            handler?.Invoke(this, new DriveCommandEventArgs(command));
        }
    }
}