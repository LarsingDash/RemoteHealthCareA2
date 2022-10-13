using Avans.TI.BLE;

namespace ClientApplication.ServerConnection.Bike;

public class BluetoothDevice
{
    private BLE ble;
    private string deviceName;
    private string? service;
    private string? characteristic;
    private BLESubscriptionValueChangedEventHandler valueChanged;


    public BluetoothDevice(string name, string? service, string? characteristic, BLESubscriptionValueChangedEventHandler? valueChanged)
    {
        deviceName = name;
        this.service = service;
        this.characteristic = characteristic;
        this.valueChanged = valueChanged;
        ble = new BLE();

    }

    /// <summary>
    /// It opens a connection to the device, sets the service, subscribes to the characteristic, and then prints if the connection
    /// has been made.
    /// </summary>
    public async Task StartConnection()
    {
        Console.WriteLine($"Starting connection of device {deviceName}");
        Thread.Sleep(1000);
        int errorCode = 0;
        errorCode = await ble.OpenDevice(deviceName);
        CheckErrorCode(errorCode, "OpenDevice");
        if (service != null)
        {
            errorCode = await ble.SetService(service);
            CheckErrorCode(errorCode, "Service");
            ble.SubscriptionValueChanged += valueChanged;
            if (characteristic != null)
            {
                errorCode = await ble.SubscribeToCharacteristic(characteristic);
                CheckErrorCode(errorCode, "Subscribe");
            }
        }

        Console.WriteLine(errorCode == 0 ? $"Connected to {deviceName}!" : $"Could not connect to {deviceName}");
    }

    private static void CheckErrorCode(int errorCode, string value)
    {
        if (errorCode != 0)
        {
            Console.WriteLine($"ErrorCode {value}: {errorCode}");
        }
    }
    
}