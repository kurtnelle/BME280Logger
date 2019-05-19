using Monoculture.Core.Drivers.BME280;
using System;
using System.Data.SqlClient;
using System.Device.Spi;
using System.Device.Spi.Drivers;
using System.IO;
using System.Threading;

namespace BME280Logger {
    class Program {
        static void Main(string[] args) {
            new WaitForDebugger();
            
            SpiConnectionSettings _spiConnectionSettings = new SpiConnectionSettings(AppSettings.Default.SpiBus,
                AppSettings.Default.SpiChipSelect) {
                Mode = SpiMode.Mode0,
                DataBitLength = 8
            };

            UnixSpiDevice _spiDevice = new UnixSpiDevice(_spiConnectionSettings);

            BME280Driver _bme280 = new BME280Driver(_spiDevice);

            _bme280.Initialize();

            _bme280.ChangeSettings(
                BME280SensorMode.Forced,
                BME280OverSample.X1,
                BME280OverSample.X1,
                BME280OverSample.X1,
                BME280Filter.Off);

            DateTime _lastLog = DateTime.MinValue;

            SqlConnection _con = new SqlConnection(AppSettings.Default.DBConnection);
            _con.Open();
            SqlCommand _command = new SqlCommand(Resources.InsertCommand, _con);
            _command.Parameters["Source"].Value = AppSettings.Default.Source;
            
            while (true) {
                _bme280.Update();
                string _data = $"Pressure : {_bme280.Pressure:0.0} Pa, Humidity : {_bme280.Humidity:0.00}%, Temprature : {_bme280.Temperature:0.00}°C";
                string _logData = $"[{DateTime.Now.ToString("dddd MMM dd, yyyy h:mm:ss tt")}] {_data}";

                if ((DateTime.Now - _lastLog).TotalMinutes > 1) {
                    _lastLog = DateTime.Now;
                    File.AppendAllLines("BME280.log", new string[] { _logData });
                    Console.WriteLine(_logData);
                    _command.Parameters["Id"].Value = new Guid().ToString();
                    _command.Parameters["Barometric"].Value = _bme280.Pressure;
                    _command.Parameters["Humidity"].Value = _bme280.Humidity;
                    _command.Parameters["Temperature"].Value = _bme280.Temperature;
                    _command.ExecuteNonQuery();
                }
                else {
                    Console.WriteLine(_data);
                }
                Thread.Sleep(1000);
            }
        }
    }
}