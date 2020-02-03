using System;
using System.Collections.Generic;
using System.Text;

namespace BME280Logger
{
    public class Settings
    {
        public string ConnectionString { get; set; } = null;
        public string Source { get; set; } = "Source1";
        public Nullable<double> MaxTemp { get; set; } = null;
        public Nullable<double> MinTemp { get; set; } = null;
        public Nullable<double> MinHumidity { get; set; } = null;
        public Nullable<double> MaxHumidity { get; set; } = null;
        public Nullable<double> MinPressure { get; set; } = null;
        public Nullable<double> MaxPressure { get; set; } = null;
        public Nullable<int> FastPollSpeed { get; set; } = 10;
        public int NormalPollSpeed { get; set; } = 60;
        public string WifiStation { get; set; } = string.Empty;
        public string WifiStationPassword { get; set; } = string.Empty;

    }
}
