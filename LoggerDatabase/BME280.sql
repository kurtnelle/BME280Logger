CREATE TABLE [dbo].[BME280]
(
	[Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY, 
    [Timestamp] DATETIME NOT NULL, 
    [Source] NVARCHAR(255) NOT NULL, 
    [Barometric] DECIMAL(18, 8) NULL, 
    [Humidity] DECIMAL(18, 8) NULL, 
    [Temperature] DECIMAL(18, 8) NULL
)
