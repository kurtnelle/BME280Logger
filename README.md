# BME280Logger
BME280 to MSSQL Logger on dotnet core
Original driver for TinyClr provided by : https://github.com/monoculture/Monoculture.TinyCLR.Drivers.BME280
Also utilizes the prototype library : https://github.com/dotnet/iot/tree/master/src/System.Device.Gpio

Driver was migrated from TinyClr using the I2C bus to DotNetCore using the SPI bus on a PocketBeagle.

The program will send it's data every minute to an instance of Microsoft SQL Server, so that the data can be stored and queried over
a long period of time. It is possible to easily modify the application to log to anything at all just change what the logic does.

It is my intent to query the data using Microsoft's Power BI therefore it needed to be somewhere convient. An instance of SQL Express does the trick.

Since the Pocket Beagle is wireless, the device can technically be anywhere within WIFI range to be able to send the data to SQL server. It would even be possible to send the data over the internet to a cloud Azure Database instance.
