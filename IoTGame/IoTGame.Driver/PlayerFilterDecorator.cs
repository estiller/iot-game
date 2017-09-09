using System.Threading.Tasks;

namespace IoTGame.Driver
{
    public class PlayerFilterDecorator : IDriver
    {
        private readonly string _playerId;
        private readonly IDriver _internalDriver;

        public PlayerFilterDecorator(string playerId, IDriver internalDriver)
        {
            _playerId = playerId;
            _internalDriver = internalDriver;
        }

        public Task StartAsync()
        {
            return _internalDriver.StartAsync();
        }

        public Task DriveAsync(DriveCommand drive)
        {
            return drive.PlayerId == _playerId ? _internalDriver.DriveAsync(drive) : Task.CompletedTask;
        }

        public Task StopAsync()
        {
            return _internalDriver.StopAsync();
        }
    }
}