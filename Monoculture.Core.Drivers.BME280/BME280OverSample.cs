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
    public enum BME280OverSample : byte
    {
        None = 0b000,
        X1 = 0b001,
        X2 = 0b010,
        X4 = 0b011,
        X8 = 0b100,
        X16 = 0b101
    }
}
