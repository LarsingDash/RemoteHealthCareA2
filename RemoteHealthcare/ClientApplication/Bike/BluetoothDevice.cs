using System.Collections.Generic;
using System.Threading.Tasks;
using Avans.TI.BLE;
using Shared.Log;

namespace ClientApplication.Bike;

public class BluetoothDevice
{
    public bool Connected = false;
    
    public BLE ble;
    private string deviceName;
    private string? service;
    private string? characteristic;
    private BLESubscriptionValueChangedEventHandler? valueChanged;


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
        Logger.LogMessage(LogImportance.Debug, $"Starting connection of device {deviceName}");
        //Thread.Sleep(500);
        int errorCode = 0;
        List<string> devices = ble.ListDevices();
        await Task.Delay(500);
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

        Connected = errorCode == 0;
        Logger.LogMessage(errorCode == 0 ? LogImportance.Information : LogImportance.Warn, errorCode == 0 ? $"Connected to {deviceName}!" : $"Could not connect to {deviceName}");
    }

    /// <summary>
    /// If the error code is not zero, log a warning message with the device name, the value, and the error code
    /// </summary>
    /// <param name="errorCode">The error code returned by the function.</param>
    /// <param name="value">The value to be set</param>
    private void CheckErrorCode(int errorCode, string value)
    {
        if (errorCode != 0)
        {
            Logger.LogMessage(LogImportance.Warn, $"Device: {deviceName} : ErrorCode {value} : {errorCode}");
        }
    }
    
}