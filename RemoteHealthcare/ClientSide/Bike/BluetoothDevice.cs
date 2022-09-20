using Avans.TI.BLE;

namespace ClientSide.Bike;

public class BluetoothDevice
{
    private BLE _ble;
    private string _deviceName;
    private string? _service;
    private string? _characteristic;
    private BLESubscriptionValueChangedEventHandler _valueChanged;


    public BluetoothDevice(string deviceName, string? service, string? characteristic, BLESubscriptionValueChangedEventHandler? valueChanged)
    {
        _deviceName = deviceName;
        _service = service;
        _characteristic = characteristic;
        _valueChanged = valueChanged;
        _ble = new BLE();

    }

    /// <summary>
    /// It opens a connection to the device, sets the service, subscribes to the characteristic, and then prints if the connection
    /// has been made.
    /// </summary>
    public async Task StartConnection()
    {
        Console.WriteLine($"Starting connection of device {_deviceName}");
        Thread.Sleep(1000);
        int errorCode = 0;
        errorCode = await _ble.OpenDevice(_deviceName);
        //Console.WriteLine($"ErrorCode openDevice: {errorCode}");
        if (_service != null)
        {
            //Console.WriteLine("Subscribed to service...");
            errorCode = await _ble.SetService(_service);
            //Console.WriteLine($"ErrorCode Service: {errorCode}");
            _ble.SubscriptionValueChanged += _valueChanged;
            if (_characteristic != null)
            {
                errorCode = await _ble.SubscribeToCharacteristic(_characteristic);
                //Console.WriteLine($"ErrorCode Subscribe: {errorCode}");
            }
        }

        Console.WriteLine(errorCode == 0 ? $"Connected to {_deviceName}!" : $"Could not connect to {_deviceName}");
    }
}