/*
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using System;
using System.Threading;
using System.Device.Spi;
using System.Device.Spi.Drivers;

namespace Monoculture.Core.Drivers.BME280 {
    public class BME280Driver {
        private int tFine;
        private int rawHumidity;
        private int rawPressure;
        private int rawTemperature;
        private SpiDevice device;
        private BME280CFData calibration;

        public BME280Driver(UnixSpiDevice spiDevice) {
            device = spiDevice ?? throw new ArgumentNullException(nameof(spiDevice));
        }

        public bool IsInitialized { get; private set; }

        public BME280Filter Filter { get; private set; } = BME280Filter.Off;

        public BME280SensorMode SensorMode { get; private set; } = BME280SensorMode.Normal;

        public BME280OverSample OsrTemperature { get; private set; } = BME280OverSample.X1;

        public BME280OverSample OsrPressure { get; private set; } = BME280OverSample.X1;

        public BME280OverSample OsrHumidity { get; private set; } = BME280OverSample.X1;

        public BME280StandbyTime StandbyDuration { get; private set; } = BME280StandbyTime.Ms05;


        private byte[] WriteRead(byte[] write) {
            return WriteRead(write, write.Length);
        }
        private byte[] WriteRead(byte[] write, int bytesToRead) {
            int _bufferSize = bytesToRead + write.Length;
            byte[] _readBuffer = new byte[_bufferSize], _writeBuffer = new byte[_bufferSize];
            Array.Copy(write, _writeBuffer, write.Length);
            device.TransferFullDuplex(new ReadOnlySpan<byte>(_writeBuffer), new Span<byte>(_readBuffer));
            byte[] _output = new byte[bytesToRead];
            Array.Copy(_readBuffer, write.Length, _output, 0, bytesToRead);
            return _output;
        }

        private void Write(byte[] write) {
            WriteRead(write, write.Length);
        }


        public void Initialize() {
            ChipId();

            Reset();

            LoadCalibration();

            WriteSettings();

            IsInitialized = true;
        }

        private void Reset() {
            Write(new byte[] { 0xD0, 0xE0 });
            //_device.Write(new byte[] { 0xD0, 0xE0 });
            Thread.Sleep(2);
        }

        private void ChipId() {
            byte[] buffer;
            buffer = WriteRead(new byte[] { 0xD0 });
            //_device.WriteRead(new byte[] { 0xD0 }, buffer);

            if (buffer[0] != 0x60) {
                throw new ApplicationException("Unrecognized chip");
            }
        }

        private void LoadCalibration() {
            byte[] _crcBuffer = new byte[1];

            _crcBuffer = WriteRead(new byte[] { 0xE8 }, 1);

            //_device.WriteRead(new byte[] { 0xE8 }, crcBuffer);
            byte[] _calibrationBuffer = new byte[33];


            byte[] _return = WriteRead(new byte[] { 0x88 }, 26);

            Array.Copy(_return, _calibrationBuffer, _return.Length);

            //_device.WriteRead(new byte[] { 0x88 }, 0, 1, _calibrationBuffer, 0, 26);

            _return = WriteRead(new byte[] { 0xE1 }, 7);

            Array.Copy(_return, 0, _calibrationBuffer, 26, _return.Length);

            //_device.WriteRead(new byte[] { 0xE1 }, 0, 1, _calibrationBuffer, 26, 7);

            calibration = new BME280CFData {
                T1 = BitConverter.ToUInt16(_calibrationBuffer, 0),
                T2 = BitConverter.ToInt16(_calibrationBuffer, 2),
                T3 = BitConverter.ToInt16(_calibrationBuffer, 4),
                P1 = BitConverter.ToUInt16(_calibrationBuffer, 6),
                P2 = BitConverter.ToInt16(_calibrationBuffer, 8),
                P3 = BitConverter.ToInt16(_calibrationBuffer, 10),
                P4 = BitConverter.ToInt16(_calibrationBuffer, 12),
                P5 = BitConverter.ToInt16(_calibrationBuffer, 14),
                P6 = BitConverter.ToInt16(_calibrationBuffer, 16),
                P7 = BitConverter.ToInt16(_calibrationBuffer, 18),
                P8 = BitConverter.ToInt16(_calibrationBuffer, 20),
                P9 = BitConverter.ToInt16(_calibrationBuffer, 22),
                H1 = _calibrationBuffer[25],
                H2 = BitConverter.ToInt16(_calibrationBuffer, 26),
                H3 = _calibrationBuffer[28],
                H4 = (short)((_calibrationBuffer[29] << 4) | (_calibrationBuffer[30] & 0xF)),
                H5 = (short)((_calibrationBuffer[31] << 4) | (_calibrationBuffer[30] >> 4)),
                H6 = (sbyte)_calibrationBuffer[32]
            };

            if (_crcBuffer[0] != CalculateCrc(_calibrationBuffer)) {
                throw new ApplicationException("CRC error loading configuration.");
            }
        }

        private static byte CalculateCrc(byte[] buffer) {
            uint crcReg = 0xFF;

            const byte polynomial = 0x1D;

            for (var index = 0; index < buffer.Length; index++) {
                for (byte bitNo = 0; bitNo < 8; bitNo++) {
                    byte din;

                    if (((crcReg & 0x80) > 0) ^ ((buffer[index] & 0x80) > 0))
                        din = 1;
                    else
                        din = 0;

                    crcReg = (ushort)((crcReg & 0x7F) << 1);

                    buffer[index] = (byte)((buffer[index] & 0x7F) << 1);

                    crcReg = (ushort)(crcReg ^ (polynomial * din));
                }
            }

            return (byte)(crcReg ^ 0xFF);
        }

        public void ChangeSettings(
            BME280SensorMode sensorMode = BME280SensorMode.Normal,
            BME280OverSample osrTemperature = BME280OverSample.X16,
            BME280OverSample osrPressure = BME280OverSample.X16,
            BME280OverSample osrHumidity = BME280OverSample.X16,
            BME280Filter filter = BME280Filter.Off,
            BME280StandbyTime standbyDuration = BME280StandbyTime.Ms05) {
            SensorMode = sensorMode;
            OsrPressure = osrPressure;
            OsrHumidity = osrHumidity;
            OsrTemperature = osrTemperature;
            StandbyDuration = standbyDuration;

            WriteSettings();
        }

        private void WriteSettings() {
            var humiReg = (byte)OsrHumidity;

            var measReg = (byte)(((byte)OsrTemperature << 5) |
                                 ((byte)OsrPressure << 3) |
                                 (byte)SensorMode);

            var confReg = (byte)((byte)StandbyDuration << 5 | (byte)Filter << 3 | 1);

            Write(new byte[] { 0xF2, humiReg });
            Write(new byte[] { 0xF5, confReg });
            Write(new byte[] { 0xF4, measReg });

            //_device.Write(new byte[] { 0xF2, humiReg });
            //_device.Write(new byte[] { 0xF5, confReg });
            //_device.Write(new byte[] { 0xF4, measReg });
        }

        private void TakeForcedReading() {
            var measReg = (byte)(((byte)OsrTemperature << 5) |
                                 ((byte)OsrPressure << 3) |
                                 (byte)SensorMode);

            Write(new byte[] { 0xF4, measReg });

            Thread.Sleep(2);
        }

        public void Update() {
            if (SensorMode == BME280SensorMode.Forced)
                TakeForcedReading();

            byte[] _buffer;

            _buffer = WriteRead(new byte[] { 0xF7 }, 8);

            rawHumidity = _buffer[7] | _buffer[6] << 8;

            rawPressure = _buffer[0] << 12 | _buffer[1] << 4 | _buffer[2] >> 4;

            rawTemperature = _buffer[3] << 12 | _buffer[4] << 4 | _buffer[5] >> 4;

            var var1 = rawTemperature / 16384.0 - calibration.T1 / 1024.0;

            var1 = var1 * calibration.T2;

            var var2 = rawTemperature / 131072.0 - calibration.T1 / 8192.0;

            var2 = var2 * var2 * calibration.T3;

            tFine = (int)(var1 + var2);
        }

        public double Temperature {
            get {
                const double temperatureMin = -40;
                const double temperatureMax = 85;

                var x = tFine / 5120.0;

                if (x < temperatureMin)
                    x = temperatureMin;
                else if (x > temperatureMax)
                    x = temperatureMax;

                return x;
            }
        }

        public float Pressure {
            get {
                float pressure;

                const float pressureMin = 30000.0f;
                const float pressureMax = 110000.0f;

                var var1 = tFine / 2.0f - 64_000.0f;

                var var2 = var1 * var1 * calibration.P6 / 32_768.0f;

                var2 = var2 + var1 * calibration.P5 * 2.0f;

                var2 = var2 / 4.0f + calibration.P4 * 65_536.0f;

                var var3 = calibration.P3 * var1 * var1 / 524_288.0f;

                var1 = (var3 + calibration.P2 * var1) / 524_288.0f;

                var1 = (1.0f + var1 / 32_768.0f) * calibration.P1;

                if (var1 > 0) {
                    pressure = 1_048_576.0f - rawPressure;

                    pressure = (pressure - var2 / 4_096.0f) * 6_250.0f / var1;

                    var1 = calibration.P9 * pressure * pressure / 2_147_483_648.0f;

                    var2 = pressure * calibration.P8 / 32_768.0f;

                    pressure = pressure + (var1 + var2 + calibration.P7) / 16.0f;

                    if (pressure < pressureMin)
                        pressure = pressureMin;
                    else if (pressure > pressureMax)
                        pressure = pressureMax;
                }
                else {
                    pressure = pressureMin;
                }

                return pressure;
            }
        }

        public float Humidity {
            get {
                const float humidityMin = 0.0f;
                const float humidityMax = 100.0f;

                var var1 = tFine - 76800.0f;

                var var2 = calibration.H4 * 64.0f + calibration.H5 / 16384.0f * var1;

                var var3 = rawHumidity - var2;

                var var4 = calibration.H2 / 65536.0f;

                var var5 = 1.0f + calibration.H3 / 67108864.0f * var1;

                var var6 = 1.0f + calibration.H6 / 67108864.0f * var1 * var5;

                var6 = var3 * var4 * (var5 * var6);

                var humidity = var6 * (1.0f - calibration.H1 * var6 / 524288.0f);

                if (humidity > humidityMax)
                    humidity = humidityMax;
                else if (humidity < humidityMin)
                    humidity = humidityMin;

                return humidity;
            }
        }
    }
}