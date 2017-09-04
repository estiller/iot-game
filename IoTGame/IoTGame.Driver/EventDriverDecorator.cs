using System;
using System.Threading.Tasks;
using IoTGame.GoPiGo;

namespace IoTGame.Driver
{
    public class EventDriverDecorator : IGoPiGoDriver
    {
        private readonly IGoPiGoDriver _internalDriver;

        public EventDriverDecorator(IGoPiGoDriver internalDriver)
        {
            _internalDriver = internalDriver;
        }

        public IGoPiGoRobot Robot => _internalDriver.Robot;

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

        private void OnDriveCommandAvailable(DriveCommand command)
        {
            var handler = DriveCommandAvailable;
            handler?.Invoke(this, new DriveCommandEventArgs(command));
        }
    }
}