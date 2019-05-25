namespace Monoculture.Core.Drivers.BME280
{
    internal class Constants
    {
        public const byte BME280_REGISTER_CHIPID = 0xD0;
        public const byte BME280_REGISTER_CONTROL = 0xF4;
        public const byte BME280_REGISTER_SOFTRESET = 0xE0;
        public const byte BME280_REGISTER_PRESSUREDATA = 0xF7;
    }
}
