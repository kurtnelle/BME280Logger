using Microsoft.Data.SqlClient;
using Monoculture.Core.Drivers.BME280;
using System;
using System.Device.I2c.Drivers;
using System.Device.I2c;
using System.Device.Spi;
using System.Device.Spi.Drivers;
using System.IO;
using System.Threading;

namespace BME280Logger {
    class Program {
        static void Main(string[] args) {
            //new WaitForDebugger();


            SpiConnectionSettings _spiConnectionSettings = new SpiConnectionSettings(2,
                1)
            {
                Mode = SpiMode.Mode0,
                DataBitLength = 8
            };

            UnixSpiDevice _spiDevice = new UnixSpiDevice(_spiConnectionSettings);

            BME280Driver _bme280 = new BME280Driver(_spiDevice);

            //I2cConnectionSettings _i2cConnectionSettings = new System.Device.I2c.I2cConnectionSettings(2, 0x76);
            //UnixI2cDevice _i2c = new UnixI2cDevice(_i2cConnectionSettings);
            //BME280Driver _bme280 = new BME280Driver(_i2c);

            DateTime _lastLog = DateTime.MinValue;
            string _connectionString = string.Empty;
            if (File.Exists("connectionstring.txt"))
            {
                _connectionString = File.ReadAllText("connectionstring.txt");
                Console.WriteLine("Connection string found. Connecting to DB...");
            }
            else
            {
                Console.WriteLine("connectionstring.txt not found. exiting");
                Environment.Exit(45); //connection string not found
            }

            SqlConnection _con = new SqlConnection(_connectionString);
            try
            {
                _con.Open();
                Console.WriteLine($"Connected to Server \"{_con.DataSource}\", and database \"{_con.Database}\"");
            }
            catch(SqlException ex)
            {
                Console.WriteLine($"Unable to connect using :\"{_connectionString}\" exiting");
                throw ex;
            }

            _bme280.Initialize();

            _bme280.ChangeSettings(
                BME280SensorMode.Forced,
                BME280OverSample.X1,
                BME280OverSample.X1,
                BME280OverSample.X1,
                BME280Filter.Off);

            SqlCommand _command = new SqlCommand(Resources.InsertCommand, _con);
            _command.Parameters.Add(new SqlParameter("Source", File.Exists("source.txt") ? File.ReadAllText("source.txt") : "Source1"));
            _command.Parameters.Add(new SqlParameter("Id", System.Data.SqlDbType.UniqueIdentifier));
            _command.Parameters.Add(new SqlParameter("Barometric", System.Data.SqlDbType.Decimal));
            _command.Parameters.Add(new SqlParameter("Humidity", System.Data.SqlDbType.Decimal));
            _command.Parameters.Add(new SqlParameter("Temperature", System.Data.SqlDbType.Decimal));
            int _loggingIntervalInSeconds = 60;
            while (true) {
                _bme280.Read();
                string _data = $"Pressure : {_bme280.Pressure:0.0} Pa, Humidity : {_bme280.Humidity:0.00}%, Tempreature : {_bme280.Temperature:0.00}°C";
                string _logData = $"[{DateTime.Now.ToString("dddd MMM dd, yyyy h:mm:ss tt")}] {_data}";

                if(_bme280.Temperature > 1 || _bme280.Humidity > 70)
                {
                    _loggingIntervalInSeconds = 10;
                }
                else
                {
                    _loggingIntervalInSeconds = 60;
                }

                if ((DateTime.Now - _lastLog).TotalSeconds > _loggingIntervalInSeconds) {
                    _lastLog = DateTime.Now;
                    File.AppendAllLines("BME280.log", new string[] { _logData });
                    Console.WriteLine(_logData);
                    _command.Parameters["Id"].Value = Guid.NewGuid();
                    _command.Parameters["Barometric"].Value = _bme280.Pressure;
                    _command.Parameters["Humidity"].Value = _bme280.Humidity;
                    _command.Parameters["Temperature"].Value = _bme280.Temperature;
                    try
                    {
                        if(_con.State!= System.Data.ConnectionState.Open)
                        {
                            _con.Open();
                        }
                        _command.ExecuteNonQuery();
                        _con.Close();
                    }
                    catch(SqlException sqEx)
                    {
                        Console.WriteLine("Error executing query command");
                        throw sqEx;
                    }
                }
                else {
                    Console.WriteLine(_data);
                }
                Thread.Sleep(1000);
            }
        }
    }
}