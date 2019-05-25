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

namespace Monoculture.Core.Drivers.BME280
{
    internal class BME280CFData
    {
        public ushort T1 { get; set; }
        public short T2 { get; set; }
        public short T3 { get; set; }
        public ushort P1 { get; set; }
        public short P2 { get; set; }
        public short P3 { get; set; }
        public short P4 { get; set; }
        public short P5 { get; set; }
        public short P6 { get; set; }
        public short P7 { get; set; }
        public short P8 { get; set; }
        public short P9 { get; set; }
        public byte H1 { get; set; }
        public short H2 { get; set; }
        public byte H3 { get; set; }
        public short H4 { get; set; }
        public short H5 { get; set; }
        public sbyte H6 { get; set; }

    }
}
