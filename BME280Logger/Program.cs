using Microsoft.Data.SqlClient;
using Monoculture.Core.Drivers.BME280;
using System;
using System.Device.Spi;
using System.Device.Spi.Drivers;
using System.IO;
using System.Threading;

namespace BME280Logger {
    class Program {
        static void Main(string[] args) {
            new WaitForDebugger();
            

            SpiConnectionSettings _spiConnectionSettings = new SpiConnectionSettings(2,
                1) {
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

            string _connectionString = @"Data Source=192.168.1.10\SQL2K16;Initial Catalog=LoggerDatabase;User ID=logger;Password=logger";

            SqlConnection _con = new SqlConnection(_connectionString);
            _con.Open();
            SqlCommand _command = new SqlCommand(Resources.InsertCommand, _con);
            _command.Parameters.Add(new SqlParameter("Source", "Source1"));
            _command.Parameters.Add(new SqlParameter("Id", System.Data.SqlDbType.UniqueIdentifier));
            _command.Parameters.Add(new SqlParameter("Barometric", System.Data.SqlDbType.Decimal));
            _command.Parameters.Add(new SqlParameter("Humidity", System.Data.SqlDbType.Decimal));
            _command.Parameters.Add(new SqlParameter("Temperature", System.Data.SqlDbType.Decimal));

            while (true) {
                _bme280.Read();
                string _data = $"Pressure : {_bme280.Pressure:0.0} Pa, Humidity : {_bme280.Humidity:0.00}%, Temprature : {_bme280.Temperature:0.00}°C";
                string _logData = $"[{DateTime.Now.ToString("dddd MMM dd, yyyy h:mm:ss tt")}] {_data}";

                if ((DateTime.Now - _lastLog).TotalMinutes > 1) {
                    _lastLog = DateTime.Now;
                    File.AppendAllLines("BME280.log", new string[] { _logData });
                    Console.WriteLine(_logData);
                    _command.Parameters["Id"].Value = Guid.NewGuid();
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