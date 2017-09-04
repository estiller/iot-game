using System;
using System.Threading;
using System.Threading.Tasks;
using IoTGame.GoPiGo;

namespace IoTGame.Driver
{
    public class DistanceMeasurementDriver : IGoPiGoDriver
    {
        private readonly IGoPiGoDriver _internalDriver;
        private readonly TimeSpan _measurementInterval;
        private Timer _timer;

        public DistanceMeasurementDriver(IGoPiGoDriver internalDriver, TimeSpan measurementInterval)
        {
            _internalDriver = internalDriver;
            _measurementInterval = measurementInterval;
        }

        public IGoPiGoRobot Robot => _internalDriver.Robot;

        public async Task StartAsync()
        {
            await _internalDriver.StartAsync();
            _timer = new Timer(MeasureDistance, null, _measurementInterval, _measurementInterval);
        }

        public Task DriveAsync(DriveCommand command)
        {
            return _internalDriver.DriveAsync(command);
        }

        public Task StopAsync()
        {
            _timer.Dispose();
            _timer = null;
            return _internalDriver.StopAsync();
        }

        public event EventHandler<DistanceMeasurementEventArgs> DistanceMeasurementAvailable;

        private void OnDistanceMeasurementAvailable(int distance)
        {
            var handler = DistanceMeasurementAvailable;
            handler?.Invoke(this, new DistanceMeasurementEventArgs(distance));
        }

        private async void MeasureDistance(object state)
        {
            var distance = await _internalDriver.Robot.DistanceSensor.MeasureInCentimetersAsync();
            OnDistanceMeasurementAvailable(distance);
        }
    }
}