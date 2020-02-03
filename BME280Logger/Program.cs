using Microsoft.Data.SqlClient;
using Monoculture.Core.Drivers.BME280;
using System;
using System.Device.I2c.Drivers;
using System.Device.I2c;
using System.Device.Spi;
using System.Device.Spi.Drivers;
using System.IO;
using System.Threading;
using Newtonsoft.Json;

namespace BME280Logger {
    class Program {
        static void Main(string[] args) {
            //new WaitForDebugger();
            if (!File.Exists("settings.json"))
            {
                Console.WriteLine("settings.json was not found. outputting default file and exiting.");
                File.WriteAllText("settings.json", JsonConvert.SerializeObject(new Settings()));
                Environment.Exit(25); //no settings json
            }
            Settings settings = JsonConvert.DeserializeObject<Settings>(File.ReadAllText("settings.json"));


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
            if (!string.IsNullOrEmpty(settings.ConnectionString))
            {
                Console.WriteLine("Connection string found. Connecting to DB...");
            }
            else
            {
                Console.WriteLine("Connection string not found. exiting");
                Environment.Exit(45); //connection string not found
            }

            SqlConnection _con = new SqlConnection(settings.ConnectionString);
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
            _command.Parameters.Add(new SqlParameter("Source", settings.Source));
            _command.Parameters.Add(new SqlParameter("Id", System.Data.SqlDbType.UniqueIdentifier));
            _command.Parameters.Add(new SqlParameter("Barometric", System.Data.SqlDbType.Decimal));
            _command.Parameters.Add(new SqlParameter("Humidity", System.Data.SqlDbType.Decimal));
            _command.Parameters.Add(new SqlParameter("Temperature", System.Data.SqlDbType.Decimal));
            int _loggingIntervalInSeconds = settings.NormalPollSpeed;
            while (true) {
                _bme280.Read();
                string _data = $"Pressure : {_bme280.Pressure:0.0} Pa, Humidity : {_bme280.Humidity:0.00}%, Tempreature : {_bme280.Temperature:0.00}°C";
                string _logData = $"[{DateTime.Now.ToString("dddd MMM dd, yyyy h:mm:ss tt")}] {_data}";

                if (settings.FastPollSpeed.HasValue)
                {
                    if((settings.MinTemp.HasValue && _bme280.Temperature <= settings.MinTemp.Value) ||
                       (settings.MaxTemp.HasValue && _bme280.Temperature >= settings.MaxTemp.Value) ||
                       (settings.MinHumidity.HasValue && _bme280.Humidity <= settings.MinHumidity.Value)||
                       (settings.MaxHumidity.HasValue && _bme280.Humidity >= settings.MaxHumidity.Value)||
                       (settings.MinPressure.HasValue && _bme280.Pressure <= settings.MinPressure.Value)||
                       (settings.MaxPressure.HasValue && _bme280.Pressure >= settings.MaxPressure.Value))
                    {
                        _loggingIntervalInSeconds = settings.FastPollSpeed.Value;
                    }
                    else
                    {
                        _loggingIntervalInSeconds = settings.NormalPollSpeed;
                    }
                }

                if ((DateTime.Now - _lastLog).TotalSeconds > _loggingIntervalInSeconds) {
                    _lastLog = DateTime.Now;
                    //File.AppendAllLines("BME280.log", new string[] { _logData });
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
                    catch(SqlException sqlEx)
                    {
                        Console.WriteLine("Error executing query command..");
                        Console.WriteLine(sqlEx.Message);
                        //throw sqEx;
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