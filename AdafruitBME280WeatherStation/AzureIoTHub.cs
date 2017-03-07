using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Client;
using Newtonsoft.Json;

static class AzureIoTHub
{
    //
    // Note: this connection string is specific to the device "WeatherStation". To configure other devices,
    // see information on iothub-explorer at http://aka.ms/iothubgetstartedVSCS
    //
    const string deviceConnectionString = "HostName=WeatherStation-v1.azure-devices.net;DeviceId=WeatherStation;SharedAccessSignature=SharedAccessSignature sr=WeatherStation-v1.azure-devices.net%2Fdevices%2FWeatherStation&sig=eeEQCI2LCclgd755T5jPYa0Z2KraMgjcUEAWuNsSPhY%3D&se=1489780914";

    //
    // To monitor messages sent to device "WeatherStation" use iothub-explorer as follows:
    //    iothub-explorer HostName=WeatherStation-v1.azure-devices.net;SharedAccessKeyName=service;SharedAccessKey=yC6meU56L/mH08vtDVzB21Lo5X5H1QIomlHF31FzC6Q= monitor-events "WeatherStation"
    //

    // Refer to http://aka.ms/azure-iot-hub-vs-cs-wiki for more information on Connected Service for Azure IoT Hub

    public static async Task SendDeviceToCloudMessageAsync(float temp, float humidity, float pressure, float altitude)
    {
        var deviceClient = DeviceClient.CreateFromConnectionString(deviceConnectionString, TransportType.Amqp);
        DateTime pollingtime = DateTime.Now;
        string sqlformattedtime = pollingtime.ToString("yyy-MM-dd HH:mm:ss.fff");
        var str = new
        {
            temp = temp,
            humidity = humidity,
            pressure = pressure,
            altitude = altitude,
            Sqlformattedtime = sqlformattedtime

        };

        var messageString = JsonConvert.SerializeObject(str);
        var message = new Message(Encoding.ASCII.GetBytes(messageString));

        await deviceClient.SendEventAsync(message).AsTask();
    }

    public static async Task<string> ReceiveCloudToDeviceMessageAsync()
    {
        var deviceClient = DeviceClient.CreateFromConnectionString(deviceConnectionString, TransportType.Amqp);

        while (true)
        {
            var receivedMessage = await deviceClient.ReceiveAsync();

            if (receivedMessage != null)
            {
                var messageData = Encoding.ASCII.GetString(receivedMessage.GetBytes());
                await deviceClient.CompleteAsync(receivedMessage);
                return messageData;
            }

            await Task.Delay(TimeSpan.FromSeconds(1));
        }
    }
}
