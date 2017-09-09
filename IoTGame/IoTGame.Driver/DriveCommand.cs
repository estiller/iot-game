using Newtonsoft.Json;

namespace IoTGame.Driver
{
    public class DriveCommand
    {
        [JsonProperty("playerid")]
        public string PlayerId { get; set; }
        [JsonProperty("motionvector")]
        public Vector MotionVector { get; set; }
        [JsonProperty("sensorvector")]
        public Vector SensorVector { get; set; }
    }
}