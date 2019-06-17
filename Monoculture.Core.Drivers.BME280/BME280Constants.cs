namespace Monoculture.Core.Drivers.BME280
{
    internal class BME280Constants
    {
        public const byte BME280_REG_HUM_LSB = 0xFE;
        public const byte BME280_REG_HUM_MSB = 0xFD;
        public const byte BME280_REG_TMP_XSB = 0xFC;
        public const byte BME280_REG_TMP_LSB = 0xFB;
        public const byte BME280_REG_TMP_MSB = 0xFA;
        public const byte BME280_REG_PRE_XSB = 0xF9;
        public const byte BME280_REG_PRE_LSB = 0xF8;
        public const byte BME280_REG_PRE_MSB = 0xF7;
        public const byte BME280_REG_CONFIG = 0xF5;
        public const byte BME280_REG_CTRL_MEAS = 0xF4;
        public const byte BME280_REG_STATUS = 0xF3;
        public const byte BME280_REG_CTRL_HUM = 0xF2;
        public const byte BME280_REG_RESET = 0xE0;
        public const byte BME280_REG_CHIPID = 0xD0;

        public const byte BME280_CRC_DATA_ADDR = 0xE8;
        public const byte BME280_CRC_DATA_LEN = 1;
        public const byte BME280_CRC_CALIB1_ADDR = 0x88;
        public const byte BME280_CRC_CALIB1_LEN = 26;
        public const byte BME280_CRC_CALIB2_ADDR = 0xE1;
        public const byte BME280_CRC_CALIB2_LEN = 7;

        public const byte BME280_CMD_SOFTRESET = 0xB6;

        public const byte BME280_CHIP_ID = 0x60;


    }
}
