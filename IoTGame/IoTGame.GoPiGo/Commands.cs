namespace IoTGame.GoPiGo
{
    public static class Commands
    {
        public const byte Version = 20;
        public const byte BatteryVoltage = 118;
        public const byte UltraSonic = 117;

        public const byte DigitalWrite = 12;
        public const byte DigitalRead = 13;
        public const byte AnalogRead = 14;
        public const byte AnalogWrite = 15;
        public const byte PinMode = 16;
        public const byte MoveForward = 119;
        public const byte MoveForwardNoPid = 105;
        public const byte MoveBackward = 115;
        public const byte MoveBackwardNoPid = 107;
        public const byte MoveLeft = 97;
        public const byte RotateLeft = 98;
        public const byte MoveRight = 100;
        public const byte RotateRight = 110;
        public const byte Stop = 120;
        public const byte IncreaseSpeedBy10 = 116;
        public const byte DecreaseSpeedBy10 = 103;
        public const byte MotorOne = 111;
        public const byte MotorTwo = 112;
        public const byte SetLeftMotorSpeed = 70;
        public const byte SetRightMotorSpeed = 71;

        public const byte RotateServo = 101;
        public const byte EnableServo = 61;
        public const byte DisableServo = 60;

        public const byte SetEncoderTargeting = 50;
        public const byte EnableEncoder = 51;
        public const byte DisableEncoder = 52;
        public const byte ReadEncoder = 53;

        public const byte EnableCommunicationTimeout = 80;
        public const byte DisableCommunicationTimeout = 81;
        public const byte ReadTimeoutStatus = 82;

        public const byte TrimTest = 30;
        public const byte WriteTrim = 31;
        public const byte ReadTrim = 32;
    }

}