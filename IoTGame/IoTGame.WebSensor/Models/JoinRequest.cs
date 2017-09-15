using System.ComponentModel.DataAnnotations;

namespace IoTGame.WebSensor.Models
{
    public class JoinRequest
    {
        [Required]
        public string Email { get; set; }
    }
}