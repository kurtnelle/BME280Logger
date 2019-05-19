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

namespace Monoculture.Core.Drivers.BME280 {
    public enum BME280StandbyTime : byte {
        Ms05 = 0b000,
        Ms10 = 0b110,
        Ms20 = 0b111,
        Ms625 = 0b001,
        Ms125 = 0b010,
        Ms250 = 0b011,
        Ms500 = 0b100,
        Ms1000 = 0b101
    }
}