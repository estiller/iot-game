using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace IoTGame.Driver
{
    public class DriveCommand
    {
        [JsonProperty("motionvector")]
        public Vector MotionVector { get; set; }
        [JsonProperty("sensorvector")]
        public Vector SensorVector { get; set; }
    }
}