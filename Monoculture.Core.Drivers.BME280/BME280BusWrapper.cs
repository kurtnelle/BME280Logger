/*
 * Author: Monoculture 2019
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
using System.Device.I2c;
using System.Device.I2c.Drivers;
using System.Device.Spi;
using System.Device.Spi.Drivers;

namespace Monoculture.Core.Drivers.BME280
{
    internal class BME280BusWrapper
    {
        public BME280BusWrapper(SpiDevice device)
        {
            SpiDevice = device;
        }

        public BME280BusWrapper(I2cDevice device)
        {
            I2CDevice = device;
        }

        private I2cDevice I2CDevice { get; }

        private SpiDevice SpiDevice { get; }

        public BME280BusType BusType => SpiDevice == null ? BME280BusType.I2C : BME280BusType.Spi;

        public byte ReadRegister(byte address)
        {
            return ReadRegion(address, 1)[0];
        }

        public byte[] ReadRegion(byte address, int length)
        {
            if (BusType == BME280BusType.I2C)
            {
                var readBuffer = new byte[length];

                var writeBuffer = new byte[] { address };

                I2CDevice.Write(writeBuffer);
                I2CDevice.Read(readBuffer);

                return readBuffer;
            }
            else
            {
                var bufferSize = length + 1;

                var txBuffer = new byte[bufferSize];

                txBuffer[0] = (byte)(address | 0x80);

                byte[] rxBuffer = new byte[txBuffer.Length];

                SpiDevice.TransferFullDuplex(txBuffer, rxBuffer);

                var output = new byte[length];

                Array.Copy(rxBuffer, 1, output, 0, output.Length);

                return output;
            }
        }

        public void WriteRegister(byte address, byte data)
        {
            WriteRegion(address, new byte[] { data });
        }

        public void WriteRegion(byte address, byte[] data)
        {
            if (BusType == BME280BusType.I2C)
            {
                var writeBuffer = new byte[data.Length + 1];

                writeBuffer[0] = address;

                Array.Copy(data, 0, writeBuffer, 1, data.Length);

                I2CDevice.Write(new ReadOnlySpan<byte>(writeBuffer));
            }
            else
            {
                var writeBuffer = new byte[data.Length + 1];

                writeBuffer[0] = (byte) (address & ~0x80); 

                Array.Copy(data, 0, writeBuffer, 1, data.Length);

                SpiDevice.Write(writeBuffer);
            }
        }
    }
}
