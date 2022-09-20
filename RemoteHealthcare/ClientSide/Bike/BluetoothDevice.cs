using Avans.TI.BLE;

namespace ClientSide.Bike;

public class BluetoothDevice
{
    private BLE ble;
    private string deviceName;
    private string? service;
    private string? characteristic;
    private BLESubscriptionValueChangedEventHandler valueChanged;


    public BluetoothDevice(string deviceName, string? service, string? characteristic, BLESubscriptionValueChangedEventHandler? valueChanged)
    {
        this.deviceName = deviceName;
        this.service = service;
        this.characteristic = characteristic;
        this.valueChanged = valueChanged;
        ble = new BLE();
        Thread.Sleep(1000);
        List<String> bleBikeList = ble.ListDevices();
        Console.WriteLine("Devices found: ");
        foreach (var name in bleBikeList)
        {
            Console.WriteLine($"Device: {name}");
        }
        
    }

    public BluetoothDevice(string deviceName, BLESubscriptionValueChangedEventHandler valueChanged)
    {
        this.deviceName = deviceName;
        this.service = null;
        this.characteristic = null;
        this.valueChanged = valueChanged;
        ble = new BLE();
        
    }

    /// <summary>
    /// It opens a connection to the device, sets the service, subscribes to the characteristic, and then prints out a
    /// message to the console
    /// </summary>
    public async Task StartConnection()
    {
        Console.WriteLine($"Starting connection of device {deviceName}");
        Thread.Sleep(1000);
        int errorCode = 0;
        errorCode = await ble.OpenDevice(deviceName);
        Console.WriteLine($"ErrorCode openDevice: {errorCode}");
        if (service != null)
        {
            //Console.WriteLine("Subscribed to service...");
            errorCode = await ble.SetService(service);
            Console.WriteLine($"ErrorCode Service: {errorCode}");
            ble.SubscriptionValueChanged += valueChanged;
            if (characteristic != null)
            {
                errorCode = await ble.SubscribeToCharacteristic(characteristic);
                //Console.WriteLine($"ErrorCode Subscribe: {errorCode}");
            }
        }

        if (errorCode == 0)
        {
            Console.WriteLine($"Connected to {deviceName}!");
        }
        else
        {
            Console.WriteLine($"Could not connect to {deviceName}");
        }



    }
}